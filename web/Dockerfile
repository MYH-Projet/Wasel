FROM node:22-alpine

WORKDIR /app

# 1. Copy everything from your 'web' folder into the container
COPY . .

# 2. Install dependencies (Simple and direct)
RUN npm install

# 3. Open the port
EXPOSE 4321

# 4. Run the dev server on host 0.0.0.0 so it's accessible outside the container
CMD ["npm", "run", "dev", "--", "--host", "0.0.0.0"]