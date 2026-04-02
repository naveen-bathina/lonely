export default function PrivacyPage() {
  return (
    <main className="mx-auto max-w-3xl px-6 py-16 text-zinc-800">
      <h1 className="text-4xl font-bold">Privacy Policy</h1>
      <p className="mt-4 text-zinc-500 text-sm">Last updated: 2026-01-01</p>

      <section className="mt-10">
        <h2 className="text-2xl font-semibold">GDPR (General Data Protection Regulation)</h2>
        <p className="mt-3 text-zinc-600 leading-7">
          If you are located in the European Economic Area (EEA), you have rights under the GDPR
          including access, rectification, erasure, and data portability. We collect only the minimum
          data required to provide the service. Consent is obtained explicitly at registration and can
          be withdrawn at any time by deleting your account.
        </p>
      </section>

      <section className="mt-10">
        <h2 className="text-2xl font-semibold">CCPA (California Consumer Privacy Act)</h2>
        <p className="mt-3 text-zinc-600 leading-7">
          California residents have the right to know what personal information we collect, the right
          to delete it, and the right to opt out of its sale. We do not sell personal information to
          third parties. To exercise your rights, contact privacy@lonely.app.
        </p>
      </section>

      <section className="mt-10">
        <h2 className="text-2xl font-semibold">Data We Collect</h2>
        <ul className="mt-3 list-disc list-inside text-zinc-600 space-y-1">
          <li>Email address or phone number (registration)</li>
          <li>Date of birth (age verification — stored as age, not raw DOB)</li>
          <li>Profile photos (moderated before publishing)</li>
          <li>Dating preferences and questionnaire responses</li>
          <li>Location (city-level only; exact location never stored)</li>
        </ul>
      </section>

      <section className="mt-10">
        <h2 className="text-2xl font-semibold">Contact</h2>
        <p className="mt-3 text-zinc-600">
          For privacy inquiries: <a href="mailto:privacy@lonely.app" className="text-rose-600 underline">privacy@lonely.app</a>
        </p>
      </section>
    </main>
  );
}
