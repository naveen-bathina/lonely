# Plan: Adult Dating App — Lonely

> Source PRD: `requirements/prds/adult-dating-app-prd.md`

## Architectural Decisions

Durable decisions that apply across all phases:

- **Platform**: Mobile-first (React Native or Flutter); API-first backend.
- **Auth**: JWT-based; email + phone registration; 18+ age gate enforced at signup.
- **API style**: RESTful, versioned under `/api/v1/`.
- **Real-time**: WebSocket (or SSE) for chat messages, match notifications, proposal/scheduling events.
- **Routes**:
  - `/api/v1/auth/*` — registration, login, token refresh, password recovery
  - `/api/v1/profiles/*` — profile CRUD, photo upload, preferences
  - `/api/v1/discover/*` — swipe feed, list feed, daily recommendations
  - `/api/v1/matches/*` — match management, mutual match detection
  - `/api/v1/messages/*` — chat threads, real-time messaging
  - `/api/v1/meetups/*` — proposal lifecycle, scheduling session
  - `/api/v1/venues/*` — venue catalog (read for users, write for admins)
  - `/api/v1/reservations/*` — reservation creation, status, cancellation
  - `/api/v1/payments/*` — split payment initiation and refund
  - `/api/v1/safety/*` — check-in state, emergency contact
  - `/api/v1/admin/*` — moderation queue, analytics, venue management
- **Key models**: `User`, `Profile`, `Preference`, `Match`, `Message`, `Verification`, `MeetupProposal`, `SchedulingSession`, `Venue`, `Reservation`, `Payment`, `SafetyCheckIn`, `Report`, `Subscription`.
- **Schema principles**:
  - `Profile` carries `verified_at`, `status`, `dating_goals`, `visibility_mode`.
  - `MeetupProposal` carries `state` enum: `pending | accepted | declined | expired`.
  - `SchedulingSession` carries `state` enum: `open | confirmed | cancelled`.
  - `Reservation` carries `state` enum: `pending_payment | confirmed | cancelled | rescheduled`.
  - `SafetyCheckIn` carries `state` enum: `pending | checked_in | missed | escalated`.
- **Privacy defaults**: profile hidden until explicitly published; location consent explicit; emergency contact opt-in with clear disclosure.
- **Compliance**: GDPR/CCPA consent stored per-user at signup; data deletion endpoint required.

---

## Phase 1: Auth & Identity

**User stories**: #1 (register email/phone), #2 (optional identity verification), #20 (GDPR/CCPA transparency)

### What to build

A complete end-to-end authentication slice: a user can sign up with email or phone number, confirm their identity via OTP, pass the 18+ age gate, and optionally submit identity verification documents. GDPR/CCPA consent is captured and stored at signup. A returning user can log in, recover their password/phone, and receive a fresh JWT. This slice touches every layer — schema, API, push notification (OTP), and UI screens (signup, login, recovery, consent modal).

### Acceptance criteria

- [ ] User can register with email or phone; OTP confirmation required before account is active.
- [ ] 18+ age gate is enforced; underage attempts are rejected with a clear message.
- [ ] GDPR/CCPA consent is presented and stored; account cannot be created without it.
- [ ] User can log in and receive a JWT; token refresh works.
- [ ] User can recover access via email/phone password reset flow.
- [ ] Optional identity verification can be submitted (document upload); `verified_at` is set on approval.
- [ ] All auth API endpoints return appropriate error codes for invalid/expired tokens.

---

## Phase 2: Profile Setup

**User stories**: #3 (dating preferences), #4 (personality questionnaire), #5 (photo upload + moderation), #6 (profile badges)

### What to build

After authentication, a user completes their profile: they set dating preferences (age range, distance, interests, dating goals), fill in a personality questionnaire, and upload photos. Uploaded photos pass through an automated moderation pipeline before being visible. Profile badges (Verified, New, Active) are computed and displayed. This slice delivers a fully navigable profile creation and editing flow end-to-end.

### Acceptance criteria

- [ ] User can set and update dating preferences (age, distance, interests, goals).
- [ ] Personality questionnaire is presented and responses stored against the profile.
- [ ] User can upload up to N photos; each photo enters a moderation queue before becoming visible.
- [ ] Moderation pipeline flags or approves photos; rejected photos notify the user.
- [ ] Profile badges (Verified, New, Active) are computed server-side and displayed on profile cards.
- [ ] Incomplete profiles are not shown in discovery.

---

## Phase 3: Discovery & Matching

**User stories**: #7 (swipe + list modes), #8 (mutual match + chat unlock), #12 (private mode), #17 (recently active), #21 (daily recommendations)

### What to build

The core discovery loop: a user sees a ranked feed of profiles (swipe card UI and list UI), likes or passes, and when both users like each other a match is created and the chat thread is unlocked. Daily recommended profiles surface at the top of the feed. Private mode hides a user's profile from the feed unless they have liked that viewer. "Recently active" timestamps are shown on profile cards. This slice is demoable as a complete match-making loop.

### Acceptance criteria

- [ ] Swipe (card stack) and list views both render profiles filtered by the user's preferences.
- [ ] Like/pass actions are recorded; a mutual like creates a `Match` record instantly.
- [ ] On mutual match, both users receive a notification and a chat thread is unlocked.
- [ ] Private mode prevents a profile from appearing in any feed until the private-mode user has liked the viewer.
- [ ] "Recently active" indicator reflects last-seen timestamp on profile cards.
- [ ] Daily recommended profiles (rule-based scoring) appear at the top of the feed.
- [ ] Profiles that are blocked by or have blocked the viewing user never appear in feeds.

---

## Phase 4: Messaging & In-chat Safety

**User stories**: #8 (chat), #9 (voice/video), #10 (ghosting reminders), #11 (block/report), #16 (read receipts toggle), #25 (reports history)

### What to build

Real-time chat inside a matched pair's thread. Messages are delivered over WebSocket. Either user can turn off read receipts for their own comfort. A block/report action is accessible from both the chat screen and profile page; reported content enters the moderation queue. Ghosting-prevention reminders nudge users who have not replied in N days. Voice/video call can be initiated from within the chat thread. Users can view their own past moderation report history.

### Acceptance criteria

- [ ] Messages are delivered in real time via WebSocket within a matched thread.
- [ ] Read receipts are shown by default; either user can disable them for themselves.
- [ ] Block action immediately removes the pair from each other's feeds and disables the chat thread.
- [ ] Report action submits content to the moderation queue and notifies the reporter.
- [ ] Ghosting-prevention reminder is triggered after a configurable idle period in an active thread.
- [ ] Voice/video call can be initiated from a chat thread; both users must accept.
- [ ] User can view a chronological history of all reports they have submitted.

---

## Phase 5: Moderation & Admin Core

**User stories**: #18 (admin moderation tools), #19 (analytics dashboard)

### What to build

An admin-only panel with two capabilities: a moderation queue to review flagged text/image content and take actions (approve, remove, warn, ban), and an analytics dashboard showing key metrics (retention, match conversion rate, safety case volume). All moderation actions are logged with admin ID and timestamp. This phase makes the platform safe to operate.

### Acceptance criteria

- [ ] Admin can view all flagged content (text messages, profile photos) in a queue, sorted by severity.
- [ ] Admin can approve, remove, warn, or ban from each queue item.
- [ ] Every moderation action is audit-logged (admin, action, timestamp, target).
- [ ] Affected users receive an in-app notification of moderation decisions.
- [ ] Analytics dashboard shows: DAU/MAU, new registrations, match conversion rate, safety case count, and report resolution time.
- [ ] Analytics data is scoped to configurable date ranges.

---

## Phase 6: Meetup Proposal

**User stories**: #26 (send proposal), #27 (accept/decline), #28 (acceptance notification), #29 (auto-expiry), #30 (proposal status)

### What to build

From inside a matched chat thread, either user can send a meetup proposal — a lightweight intent signal with no venue or time attached. The other user receives a push notification and can accept or decline. If no response arrives within a configurable TTL, the proposal auto-expires. The proposer can see the current state (pending / accepted / declined / expired) at any time. Accepting a proposal unlocks the Scheduling phase (Phase 8). This is a thin but complete vertical slice through the `MeetupProposal` model, API, notification, and UI card.

### Acceptance criteria

- [ ] Either matched user can send one active meetup proposal per match at a time.
- [ ] Recipient receives a push notification for an incoming proposal.
- [ ] Recipient can accept or decline; proposer is notified of the decision.
- [ ] Proposal auto-expires after a configurable TTL with no response; both users are notified.
- [ ] Proposal state (pending / accepted / declined / expired) is visible to both users in the chat thread.
- [ ] Accepting a proposal transitions the match into the Scheduling phase.
- [ ] A declined or expired proposal can be re-sent after a cooldown period.

---

## Phase 7: Venue Catalog

**User stories**: #31 (browse venues), #33 (venue detail view), #43 (admin venue CRUD), #44 (admin reservation activity)

### What to build

An admin-curated venue catalog. Admins can create, edit, and remove venues (name, type, address, photos, capacity, amenities, availability slots). Users in the Scheduling phase can browse venues filtered by type, distance, and availability, and view a detail page for any venue. Admins can also view reservation activity per venue and flag problematic venues. This delivers the data layer and UI for venue selection ahead of the scheduling flow.

### Acceptance criteria

- [ ] Admin can create, edit, and delete venues with all required fields.
- [ ] Venue availability slots can be configured per venue by admin.
- [ ] Users can browse the venue catalog filtered by type, distance, and availability.
- [ ] Users can view a full venue detail page (photos, address, amenities, capacity).
- [ ] Admin can view a list of all reservations associated with a venue.
- [ ] Admin can flag a venue as under review, which hides it from the user-facing catalog.

---

## Phase 8: Meetup Scheduling & Reservation

**User stories**: #32 (suggest venue + time), #34 (counter-suggest), #35 (final confirmation), #36 (reservation gate), #37 (split payment), #38 (confirmation notification), #39 (cancel/reschedule)

### What to build

The full collaborative scheduling flow, unlocked when a meetup proposal is accepted. Either user can suggest a venue and date/time from the catalog. The other user can accept the suggestion or counter-suggest with a different venue or time. Once both users give explicit final confirmation, a `Reservation` is created, split payment is collected from both users (each pays their half), and a confirmation notification is sent to both. Either user can cancel or reschedule within a defined window; cancellation policies and refunds are handled in-app.

### Acceptance criteria

- [ ] Either user in an accepted proposal can suggest a venue and date/time to start the scheduling session.
- [ ] The other user can accept the suggestion or make a counter-suggestion.
- [ ] The scheduling session shows the current pending suggestion and its origin at all times.
- [ ] A reservation is only created after both users explicitly confirm the same venue and time.
- [ ] Split payment is collected from both users at the moment of mutual confirmation.
- [ ] Both users receive a reservation confirmation notification with venue details and time.
- [ ] Either user can cancel or reschedule within the venue's cancellation window; refund is processed if applicable.
- [ ] Cancellation outside the window results in no refund per venue policy.

---

## Phase 9: Meetup Safety Check-in

**User stories**: #40 (arrival check-in), #41 (post-meetup check-in), #42 (missed check-in escalation)

### What to build

A safety layer around confirmed meetups. During onboarding (or from settings), users provide an emergency contact. When a reservation's start time arrives, the app sends a check-in push to the user. The user marks themselves as safe. After the meetup's expected end window, a post-meetup check-in is sent. If either check-in is missed within N minutes, an escalation notification is sent to the emergency contact. The `SafetyCheckIn` state machine drives all transitions.

### Acceptance criteria

- [ ] Users can add and update an emergency contact (name + phone/email) with explicit consent disclosure.
- [ ] An arrival check-in push is sent at the reservation start time.
- [ ] User can mark themselves safe; check-in state transitions to `checked_in`.
- [ ] A post-meetup check-in push is sent after a configurable post-meetup window.
- [ ] If a check-in is not acknowledged within N minutes, state transitions to `missed` and an escalation notification is sent to the emergency contact.
- [ ] All check-in state transitions are audit-logged.
- [ ] Users can disable safety check-ins per meetup with an explicit opt-out confirmation.

---

## Phase 10: Premium & Growth Features

**User stories**: #13 (event-based meetups), #14 (premium filters), #15 (subscription boost), #22 (date feedback), #23 (coaching tips), #24 (discovery pause timer)

### What to build

Monetization and growth tooling. Users can subscribe to premium tiers that unlock advanced filters (relationship type, pets, favorites) and a visibility boost. Event-based meetups (virtual or local) let users discover community beyond one-on-one matching. After a date, users can submit structured feedback to improve the community. In-app coaching tips surface on the profile and conversation screens. A discovery pause timer lets users temporarily stop appearing in feeds to avoid burnout.

### Acceptance criteria

- [ ] Subscription tiers are defined; users can subscribe and manage their plan in-app.
- [ ] Premium filters (relationship type, pets, favorites) are gated behind a subscription.
- [ ] Boost temporarily increases a user's visibility in discovery feeds for a configurable duration.
- [ ] Events can be created (admin or premium users) and browsed by all users; users can RSVP.
- [ ] Post-date feedback form is surfaced after a meetup is marked complete.
- [ ] Coaching tips appear contextually on profile and chat screens.
- [ ] Discovery pause timer hides the user from feeds for a user-selected duration; the user is notified when the timer expires.
