import { dirname } from "path";
import { fileURLToPath } from "url";

const appRoot = dirname(fileURLToPath(import.meta.url));

/** @type {import('next').NextConfig} */
const nextConfig = {
  outputFileTracingRoot: appRoot,
};

export default nextConfig;
