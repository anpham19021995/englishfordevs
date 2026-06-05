import type { Metadata } from "next";
import "./globals.css";

export const metadata: Metadata = {
  title: "English for Developers",
  description: "AI-powered English practice for software engineers",
};

export default function RootLayout({
  children,
}: Readonly<{
  children: React.ReactNode;
}>) {
  return (
    <html lang="en">
      <body>{children}</body>
    </html>
  );
}
