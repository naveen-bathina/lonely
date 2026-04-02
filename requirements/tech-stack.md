# Tech Stack

## Frontend / Mobile

| Concern | Choice | Notes |
|---|---|---|
| Framework | **Flutter (Dart)** | iOS, Android, and in-app web views |
| Public web | **Next.js (React / TypeScript)** | Landing page, shareable profile links, GDPR/compliance pages, SEO |
| State management | TBD | Riverpod or Bloc — decide before first feature slice |
| Navigation | GoRouter | Declarative, deep-link friendly |

## Backend

| Concern | Choice | Notes |
|---|---|---|
| Runtime | **.NET Core (C#)** | REST APIs, background services, SignalR hub |
| API style | REST (JSON) | gRPC available for internal service-to-service calls |
| Auth | ASP.NET Core Identity + JWT Bearer | Access + refresh token pattern; 18+ age gate enforced at registration |
| Real-time | **ASP.NET Core SignalR** | Chat, match notifications, call signalling, safety check-in alerts |
| ORM | Entity Framework Core | Code-first migrations |

## Data

| Concern | Choice | Notes |
|---|---|---|
| Primary DB | **PostgreSQL + PostGIS** | Relational store; PostGIS for geo/location queries (distance filters, nearby venues) |
| Cache / queue | **Redis** | SignalR backplane (required for multi-pod k8s), session cache, match queue, rate limiting |
| Object storage | **MinIO** (self-managed) | S3-compatible; profile photos, venue images; runs as a k8s workload |

## Infrastructure

| Concern | Choice | Notes |
|---|---|---|
| Deployment | **Kubernetes (self-managed)** | Helm charts for each service; HorizontalPodAutoscaler for real-time pods |
| Containers | Docker | Multi-stage builds; .NET runtime image + Flutter Web via nginx |
| Container registry | **GitHub Container Registry (GHCR)** | Integrated with GitHub Actions |
| CI/CD | **GitHub Actions** | Build → test → push image → deploy to cluster |
| Secrets | Kubernetes Secrets (sealed or external) | Avoid plain-text secrets in manifests |

## External Services

| Concern | Choice | Notes |
|---|---|---|
| Push notifications | **Firebase Cloud Messaging (FCM)** | iOS + Android push; safety check-in alerts, match/proposal notifications |
| Payment | **TBD** | Stripe is the recommended option for split-payment and subscription support |
| Image / text moderation | TBD | Required for photo upload and chat safety pipeline; evaluate open-source vs managed |
| Voice / video calls | TBD | WebRTC-based; evaluate open-source (LiveKit, Janus) vs managed (Daily, Twilio) |

## Testing

| Layer | Framework |
|---|---|
| .NET unit + integration | xUnit + Moq |
| .NET API tests | ASP.NET Core TestHost (WebApplicationFactory) |
| Flutter unit | flutter_test |
| Flutter integration / e2e | integration_test |
| Contract / behavior | Reqnroll (SpecFlow successor for .NET) |

## Out of Scope (for now)

- Third-party venue booking integrations (OpenTable, Yelp)
- Deep ML matchmaking (rule-based scoring in MVP)
- AI image generation

## Open Decisions

- Flutter state management: Riverpod vs Bloc
- Payment provider: Stripe is recommended; defer until Phase 3 (Meetup Facilitation)
- Image/text moderation service
- Voice/video call provider
- Kubernetes ingress controller (Nginx, Traefik, or Gateway API)
