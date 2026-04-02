## Problem Statement

Adults who are looking for dating connections lack a safe, enjoyable, privacy-first app that supports:

- clear matchmaking preferences,
- verified profiles,
- meaningful communication,
- and respectful community moderation.

Current solutions are noisy, rife with low-quality interactions, and often lack modern UX patterns for adults with diverse dating goals. Critically, no existing platform bridges the gap between online matching and in-person meetups — users are left to coordinate logistics, find neutral venues, and split costs entirely on their own, which creates friction and safety concerns.

## Solution

Build a mobile-first dating app for adults that:

- supports profile verification,
- robust matching (filters + intelligence),
- tiered privacy controls,
- structured messaging with safety features,
- and community moderation tools.

The app's core USP is **in-app meetup facilitation**: once two users match, either can propose a venue (restaurant, café, park, activity spot, etc.), reserve a table or space directly within the app, and split the cost seamlessly. Admin-curated venue listings ensure quality. A built-in safety check-in flow protects users throughout the meetup.

The app will support:

- casual dates,
- long-term relationships,
- friendship/networking,
- and local event-based discovery.

## User Stories

1. As an adult user, I want to register with email/phone so that I can create a secure account.
2. As an adult user, I want optional identity verification so that I can trust other profiles.
3. As an adult user, I want to set dating preferences (age, location, interests) so that I see relevant matches.
4. As a busy adult user, I want a personality questionnaire so that matching is more accurate.
5. As an adult user, I want to upload multiple photos with moderation to express myself safely.
6. As an adult user, I want profile badges (verified, new, active) so that I can assess intent.
7. As an adult user, I want discover-by-swipe and discover-by-list modes for different browsing styles.
8. As an adult user, I want mutual match chat unlock so that first contact is consent-based.
9. As an adult user, I want in-app voice/video call options to move beyond text safely.
10. As an adult user, I want "ghosting prevention" reminders to support healthy behavior.
11. As an adult user, I want a block/report button in chat and profile pages to flag abuse fast.
12. As an adult user, I want a "private mode" that hides my profile unless I swipe like.
13. As an adult user, I want event-based meetups (virtual/local) to find community.
14. As an adult user, I want premium filter capabilities (relationship type, pets, favorites).
15. As an adult user, I want a subscription-based "boost" to increase visibility temporarily.
16. As an adult user, I want a "read receipts off" option for personal comfort.
17. As an adult user, I want a "recently active" indicator to gauge responsiveness.
18. As an admin, I want content moderation tools for text and image safety.
19. As an admin, I want analytics for retention, match conversion, and safety cases.
20. As a user, I want local law and privacy transparency (GDPR/CCPA) to trust the service.
21. As a user, I want daily recommended profiles so I can discover better matches.
22. As a user, I want to give and get feedback after dates to improve the community.
23. As a user, I want in-app coaching tips for better profile and conversation skills.
24. As a user, I want a "skip if zzz" timer to automatically pause discovery to avoid burnout.
25. As a user, I want a “reports history” page for my past moderation actions.
--- Meetup Proposal Phase ---
26. As a matched user, I want to send a meetup proposal to my match so that I can express interest in meeting in person.
27. As a matched user, I want to accept or decline an incoming meetup proposal so that I only proceed when I'm comfortable.
28. As a matched user, I want to receive a notification when my partner accepts my meetup proposal so that I know scheduling can begin.
29. As a matched user, I want a meetup proposal to automatically expire after a set period if not responded to so that unanswered requests don't linger.
30. As a matched user, I want to see the status of my sent meetup proposal (pending, accepted, declined, expired) so that I know where things stand.

--- Meetup Scheduling Phase (unlocked after proposal is accepted) ---
31. As a matched user, I want to browse admin-curated venues (restaurants, cafes, parks, activity spots) so that I can find a suitable location.
32. As a matched user, I want to suggest a venue and preferred date/time to my partner during scheduling so that we can agree on details.
33. As a matched user, I want to view venue details (type, address, photos, capacity, amenities) before suggesting it so that I can make an informed choice.
34. As a matched user, I want my partner to be able to counter-suggest a different venue or time so that we reach a mutually comfortable plan.
35. As a matched user, I want to give final confirmation on the agreed venue and time so that both parties explicitly consent before a reservation is made.
36. As a matched user, I want a reservation to be created only after both users confirm the venue and time so that no booking is made unilaterally.
37. As a matched user, I want to split the reservation cost equally with my partner in-app at the time of confirmation so that neither party pays alone.
38. As a matched user, I want to receive a reservation confirmation notification so that I know the booking is secured.
39. As a matched user, I want to cancel or reschedule within a defined window so that plans can change without full penalty.

--- Meetup Safety ---
40. As a matched user, I want a safety check-in prompt when my meetup starts so that a trusted contact or the app is aware I've arrived.
41. As a matched user, I want a post-meetup safety check-in so that the app knows I am safe after the date.
42. As a matched user, I want escalation alerts sent to my emergency contact if I miss a safety check-in so that help can be sought quickly.

--- Admin ---
43. As an admin, I want to add, edit, and remove venues from the curated list so that only quality, vetted locations are shown.
44. As an admin, I want to view reservation activity and flag problematic venues so that the platform maintains quality standards.

## Implementation Decisions

- Modules: Auth + Verification, Profile Management, Matching Engine, Discovery UX, Messaging / Real-time comms, Safety & Moderation, Analytics + Payments, **Venue Management**, **Reservation & Scheduling**, **Split Payment**, **Meetup Safety Check-in**.
- Core interfaces: user preference model, match scoring API, messaging channel with safety pipeline, admin incident API, venue catalog API, reservation lifecycle API, split payment API, check-in state machine.
- Data-level: profile schema includes verified-check, status flags, dating goals; message meta includes moderation tags; venue schema includes type, address, availability slots, capacity; reservation schema includes status (proposed/confirmed/cancelled), participants, time, cost, split status.
- Venue catalog: admin-only write access; users have read-only browsing with filters (type, distance, availability).
- Meetup flow (two phases):
  - **Phase 1 — Proposal**: User A sends a meetup proposal (no venue attached); User B receives a notification and accepts or declines; proposal auto-expires after a configurable TTL if not responded to; proposal states: pending → accepted | declined | expired.
  - **Phase 2 — Scheduling** (unlocked only on acceptance): either user suggests a venue + date/time from the curated catalog; the other can counter-suggest; once both users give final confirmation → reservation is created → split payment collected from both → confirmation notification sent to both.
- Split payment: each user pays their half at confirmation; cancellation policy defined at venue level; refund logic handled in-app.
- Safety check-in: user sets a trusted emergency contact at onboarding; app sends check-in push at meetup start time; user marks safe; if no response within N minutes, escalation notification sent to emergency contact.
- UX: consent-first chat, explicit location consent, venue proposal flow embedded in match chat thread.
- Non-functional: 18+ age gate, privacy-first defaults, scalable architecture, API-first backend.

## Testing Decisions

- Behavior tests for registration, matching filters, mutual chat, block/report, privacy mode.
- Behavior tests for meetup proposal state machine: send proposal, accept, decline, expiry.
- Behavior tests for scheduling phase: venue suggestion, counter-suggestion, mutual confirmation, reservation creation, split payment, cancellation and refund.
- Behavior tests for safety check-in state machine: on-time check-in, missed check-in, escalation trigger.
- Modules: AuthService, RecommendationService, ChatService, ModerationService, VenueService, ReservationService, SplitPaymentService, SafetyCheckInService.
- Prior art: auth + behavior graph tests, moderation queue tests.
- Coverage: unit + integration + e2e.

## Out of Scope

- AI image generation.
- VR dating rooms.
- Third-party social graph imports.
- Deep ML matchmaking in MVP (rule-based scoring only).
- Enterprise team-building.
- Legal managed background-check service.
- Third-party venue booking integrations (e.g., OpenTable, Yelp) — custom reservation system only.
- Venue ratings and reviews by users (post-MVP).

## Further Notes

- Validate inclusivity for gender/identity in MVP.
- Decide intl/i18n roadmap.
- Delivery plan: MVP -> Safety/premium -> Growth features -> Meetup facilitation (USP).
- Venue availability management (slot booking, overbooking prevention) is a core complexity to design carefully.
- Emergency contact collection requires explicit user consent and clear privacy policy disclosure.
