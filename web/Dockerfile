# ══════════════════════════════════════════════════
# Stage 1: Build the Astro Project
# ══════════════════════════════════════════════════
FROM node:22-alpine AS builder

WORKDIR /app

# Copy package files first to cache dependencies
COPY package*.json ./
RUN npm ci

# Copy the rest of the frontend code (including tsconfig.json, src, etc.)
COPY . .

# Build the Astro project for SSR/Hybrid
RUN npm run build

# ══════════════════════════════════════════════════
# Stage 2: Production Runtime
# ══════════════════════════════════════════════════
FROM node:22-alpine AS runtime

WORKDIR /app

# Copy only the necessary files from the builder
COPY --from=builder /app/dist ./dist
COPY --from=builder /app/node_modules ./node_modules
COPY --from=builder /app/package.json ./package.json

# Environment variables for Astro Node server
ENV HOST=0.0.0.0
ENV PORT=4321
ENV NODE_ENV=dev

EXPOSE 4321

# Start the Node server
CMD ["node", "./dist/server/entry.mjs"]