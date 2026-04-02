# Architecture Decisions

All decisions reached through design review. Each entry states the decision and the rationale.

---

## 1. Backend Architecture — Modular Monolith

One .NET Core solution with bounded contexts separated as C# projects/namespaces. Single deployment unit. Migrate hot modules (Chat, Matching) to separate services only when load demands it.

**Rationale:** Microservices on day one adds massive operational overhead before product-market fit is validated.

---

## 2. API Surface — Single API

Flutter app, Next.js public web, and SignalR all served from the same .NET Core application. No BFF or API gateway in MVP.

**Rationale:** Clients don't diverge enough in MVP to justify separate API surfaces. YARP/gateway can be added later.

---

## 3. Authentication — Rotating Refresh Tokens + httpOnly Cookie

- **Flutter:** Access token (15 min) + rotating refresh token (30 days, single-use).
- **Next.js:** httpOnly cookie session for SSR pages where auth state matters. Public profile pages unauthenticated.

**Rationale:** Rotating refresh tokens are revocable and secure. httpOnly cookies are SSR-friendly for Next.js without exposing tokens to JavaScript.

---

## 4. Matching Engine — Pull on Demand

Discovery queries PostgreSQL live: apply filters (age, distance via PostGIS, interests), score, rank, paginate. No pre-computed feed in MVP.

**Rationale:** User base won't require pre-computation at MVP scale. Redis feed caching added only when latency is a measured problem.

---

## 5. Chat Storage — PostgreSQL

Messages stored in PostgreSQL, indexed on `(conversation_id, created_at DESC)`. Messages retained for match lifetime. Soft-deleted on unmatch/block. No time-based purge in MVP.

**Rationale:** Scale doesn't justify a dedicated message store. Clean index design makes migration easy later.

---

## 6. Photo Upload — Presigned URL + Async Moderation

Flutter requests a presigned MinIO URL from the API, then uploads directly to MinIO (API not in the upload path). Images stored immediately as `pending`; moderation runs asynchronously; image goes live on approval. User notified on approval/rejection.

**Rationale:** Presigned URLs keep API pods lean. Async moderation gives instant UX feedback without blocking the user.

---

## 7. Moderation Pipeline — Self-hosted Classifier + Passive Chat Scanning

- **Photos:** Self-hosted classifier (e.g. NudeNet) auto-rejects clear violations; borderline cases go to human admin review queue.
- **Chat:** Passive scanning only — flag keywords/patterns and route to admin queue. Messages are not blocked in MVP.
- **Reviewers:** Internal admin team via the admin dashboard.

**Rationale:** Self-hosted keeps all data in-cluster (privacy-first). Passive chat scanning avoids silent blocking UX issues and legal complexity.

---

## 8. Meetup Proposal — One Active Proposal Per Match

Only one proposal can be in `pending` state per match at a time. If A's proposal is pending, B sees it and responds; B cannot create a competing proposal until it resolves.

**Rationale:** Prevents race conditions and maps cleanly to the state machine in the PRD.

---

## 9. Venue Availability — Fixed Admin-defined Time Slots

Admin defines discrete bookable slots per venue (e.g. "7:00 PM – 9:00 PM"). Slot states: `available → reserved | blocked`. Reservation creation is atomic via DB transaction + optimistic locking to prevent double-booking.

**Rationale:** Eliminates overbooking races. Admin-controlled quality. Free-form datetime adds unnecessary complexity.

---

## 10. Split Payment — All-or-Nothing

Both payment intents are authorized at confirmation. Captured only when both succeed. If either fails, both are released and users are notified to retry. No debt tracking, no partial reservations.

**Rationale:** Cleanest model for trust and accounting. Avoids debt state and partial reservation edge cases.

---

## 11. Safety Check-in Escalation — Two-step with SMS

- T+0: Push check-in notification to user.
- T+10 min: Push reminder if no response.
- T+20 min: SMS escalation to emergency contact.
- Message: "[First name] listed you as their emergency contact. We haven't heard from them. Please check on them."
- No location sharing in MVP (requires separate explicit consent flow).
- Email as SMS fallback.
- SMS gateway: Twilio or self-hosted GSM gateway.

**Rationale:** Two-step reduces false positives significantly. SMS ensures emergency contact doesn't need the app installed. No location in MVP avoids consent complexity.

---

## 12. Identity Verification — Selfie Liveness Check

MVP: user submits a real-time selfie compared to their profile photo (liveness check). Badge reads "verified real person." No government ID collected or stored.

Post-MVP: optional enhanced tier with ID document matching.

**Rationale:** Liveness proves a real human. Storing government IDs creates massive GDPR/legal liability and is unnecessary for MVP trust signal.

---

## 13. Kubernetes Deployments — Three from One Codebase

Three deployments, all from the same .NET Core solution with different entry points:

| Deployment | Responsibility |
|---|---|
| `api` | REST endpoints: auth, matching, venues, reservations, profiles |
| `realtime` | ASP.NET Core SignalR hub; scales horizontally via Redis backplane |
| `worker` | Hangfire background jobs: moderation queue, safety check-in scheduler, proposal TTL expiry, ghosting reminders |

**Rationale:** Prevents background jobs from starving API threads. Allows independent scaling of real-time connections.

---

## 14. Job Scheduling — Hangfire + PostgreSQL

Hangfire backed by PostgreSQL for both recurring (cron-style) and deferred (enqueue with delay) jobs. Built-in retry, failure tracking, and admin dashboard.

**Rationale:** No new infra required (PostgreSQL already present). Handles event-triggered deferred jobs (e.g. "expire proposal X in 48h") that Kubernetes CronJobs cannot.

---

## 15. Delete Strategy — Hybrid (PII Anonymised, System Records Soft-deleted)

- **Account deletion / GDPR request:** PII fields (name, email, phone) anonymised immediately; profile photos deleted from MinIO; `anonymised_at` set on user record.
- **System records** (reservations, moderation cases, safety incidents): retain anonymised references for audit and safety purposes.
- **Blocks / unmatches:** Soft delete (`deleted_at`) — reversible.

**Rationale:** GDPR-compliant without destroying safety audit trail. Standard approach for dating platforms.

---

## 16. Private Mode Visibility Rules

- User A in private mode likes User B → B can see A's profile immediately (no mutual required).
- Likes given before private mode was enabled still count.
- Admins always see all profiles regardless of privacy settings.

**Rationale:** Private mode limits unsolicited discovery, not responses to explicit interest. Admin visibility is non-negotiable for moderation.

---

## 17. Premium Gating — Subscription Flag on User Record

`is_premium` + `premium_expires_at` on the user table. Checked server-side on every gated endpoint. Kept in sync via payment provider webhook.

**Rationale:** JWT claims for subscription state have up to 15-minute staleness — unacceptable for paid feature cancellation. Single indexed DB lookup per request is negligible.

---

## 18. Ingress & TLS — Nginx Ingress + cert-manager + Let's Encrypt

Nginx Ingress Controller for routing. cert-manager + Let's Encrypt for automatic TLS certificate provisioning and renewal.

**Rationale:** Most battle-tested ingress option for self-managed Kubernetes. Extensive documentation and community support. Free TLS via Let's Encrypt.

---

## 19. Flutter State Management — Bloc

Bloc (Business Logic Component) pattern for all Flutter state management.

**Rationale:** Explicit event/state model maps directly to the app's non-trivial state machines: meetup proposal (pending/accepted/declined/expired), reservation scheduling (suggest/counter-suggest/confirm), and safety check-in (waiting/checked-in/escalated). Auditable and testable in a safety-critical app.

---

## 20. Voice/Video Calls — LiveKit Self-hosted + coturn

LiveKit WebRTC server deployed as a Kubernetes workload. coturn for TURN/NAT traversal. Flutter SDK available.

**Rationale:** No data leaves the cluster (privacy-first). No per-minute cost. Scales on Kubernetes. Fits the self-managed infrastructure philosophy.
