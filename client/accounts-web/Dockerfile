# syntax=docker/dockerfile:1.7

FROM node:22-alpine AS build
WORKDIR /app
COPY client/accounts-web/package*.json ./
RUN npm ci
COPY client/accounts-web/ ./
RUN npm run build

FROM nginx:alpine AS runtime
COPY --from=build /app/dist /usr/share/nginx/html
COPY client/accounts-web/nginx.conf /etc/nginx/conf.d/default.conf
EXPOSE 80
