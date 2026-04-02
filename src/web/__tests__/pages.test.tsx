import { render, screen } from '@testing-library/react';
import LandingPage from '@/app/page';
import ProfilePage from '@/app/profiles/[userId]/page';
import PrivacyPage from '@/app/privacy/page';

describe('LandingPage', () => {
  it('renders the app name', () => {
    render(<LandingPage />);
    expect(screen.getByRole('heading', { name: /lonely/i })).toBeInTheDocument();
  });

  it('renders a call-to-action button', () => {
    render(<LandingPage />);
    expect(screen.getByRole('link', { name: /get started/i })).toBeInTheDocument();
  });

  it('renders the tagline', () => {
    render(<LandingPage />);
    expect(screen.getByText(/meet someone real/i)).toBeInTheDocument();
  });
});

describe('ProfilePage', () => {
  it('renders the user profile section', async () => {
    const page = await ProfilePage({ params: Promise.resolve({ userId: 'user123' }) });
    render(page);
    expect(screen.getByRole('heading', { name: /profile/i })).toBeInTheDocument();
  });

  it('renders a download app CTA', async () => {
    const page = await ProfilePage({ params: Promise.resolve({ userId: 'user123' }) });
    render(page);
    expect(screen.getByRole('link', { name: /download the app/i })).toBeInTheDocument();
  });
});

describe('PrivacyPage', () => {
  it('renders the privacy policy heading', () => {
    render(<PrivacyPage />);
    expect(screen.getByRole('heading', { name: /privacy policy/i })).toBeInTheDocument();
  });

  it('renders GDPR section', () => {
    render(<PrivacyPage />);
    expect(screen.getByRole('heading', { name: /gdpr/i })).toBeInTheDocument();
  });

  it('renders CCPA section', () => {
    render(<PrivacyPage />);
    expect(screen.getByRole('heading', { name: /ccpa/i })).toBeInTheDocument();
  });
});
