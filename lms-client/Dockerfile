FROM node:24-alpine AS build
WORKDIR /app

COPY package.json package-lock.json ./
# npm ci fails here: the Windows-generated lockfile lacks Linux-only optional peer deps (@emnapi/*)
RUN npm install --no-audit --no-fund

COPY . .
RUN npx ng build --configuration docker

FROM node:24-alpine AS final
WORKDIR /app
ENV NODE_ENV=production
ENV PORT=4000
COPY --from=build /app/dist/lms-client ./dist/lms-client
EXPOSE 4000
CMD ["node", "dist/lms-client/server/server.mjs"]
