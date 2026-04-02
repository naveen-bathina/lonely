export default function LandingPage() {
  return (
    <main className="flex min-h-screen flex-col items-center justify-center bg-rose-50 px-6 text-center">
      <h1 className="text-5xl font-bold tracking-tight text-rose-700">Lonely</h1>
      <p className="mt-4 max-w-md text-xl text-zinc-600">
        Meet someone real. Privacy-first dating for adults.
      </p>
      <div className="mt-8 flex gap-4">
        <a
          href="/register"
          className="rounded-full bg-rose-600 px-8 py-3 text-white font-semibold hover:bg-rose-700 transition-colors"
        >
          Get Started
        </a>
        <a
          href="/privacy"
          className="rounded-full border border-rose-300 px-8 py-3 text-rose-700 font-semibold hover:bg-rose-100 transition-colors"
        >
          Privacy Policy
        </a>
      </div>
      <section className="mt-16 grid max-w-3xl grid-cols-1 gap-6 text-left sm:grid-cols-3">
        <div className="rounded-2xl bg-white p-6 shadow-sm">
          <h2 className="font-semibold text-rose-700">Verified Profiles</h2>
          <p className="mt-2 text-sm text-zinc-500">Real people, identity-verified for your safety.</p>
        </div>
        <div className="rounded-2xl bg-white p-6 shadow-sm">
          <h2 className="font-semibold text-rose-700">Smart Matching</h2>
          <p className="mt-2 text-sm text-zinc-500">Preference filters and daily recommendations.</p>
        </div>
        <div className="rounded-2xl bg-white p-6 shadow-sm">
          <h2 className="font-semibold text-rose-700">Safe Meetups</h2>
          <p className="mt-2 text-sm text-zinc-500">In-app venue booking, split costs, safety check-ins.</p>
        </div>
      </section>
    </main>
  );
}
