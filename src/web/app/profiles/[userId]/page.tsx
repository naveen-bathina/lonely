type Props = { params: Promise<{ userId: string }> };

export default async function ProfilePage({ params }: Props) {
  const { userId } = await params;

  return (
    <main className="flex min-h-screen flex-col items-center justify-center bg-white px-6 text-center">
      <h1 className="text-3xl font-bold text-zinc-800">Profile</h1>
      <p className="mt-2 text-zinc-500 text-sm">User ID: {userId}</p>
      <p className="mt-4 max-w-sm text-zinc-600">
        This profile is shared from the Lonely app. Download the app to connect.
      </p>
      <a
        href="https://lonely.app/download"
        className="mt-6 rounded-full bg-rose-600 px-8 py-3 text-white font-semibold hover:bg-rose-700 transition-colors"
      >
        Download the App
      </a>
    </main>
  );
}
