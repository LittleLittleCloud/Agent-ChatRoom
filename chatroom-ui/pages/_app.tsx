import RootLayout from "@/components/root-layout";
import "@/styles/globals.css";
import type { AppProps } from "next/app";
import { useState } from "react";

export default function App({ Component, pageProps }: AppProps) {
  const [checkpoint, setCheckpoint] = useState<string | undefined>(undefined);

  const onSelectedCheckpoint = (checkpoint: string | undefined) => {
    setCheckpoint(checkpoint);
  }
  return (
    <RootLayout onSelectedCheckpoint={onSelectedCheckpoint}>
      <Component {...pageProps} checkpoint={checkpoint} />
    </RootLayout>
  )
}
