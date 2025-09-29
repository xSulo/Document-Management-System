# Build
FROM node:20-alpine AS build
WORKDIR /app
COPY ui/package*.json ./
RUN npm install
COPY ui ./
RUN npm run build

# Run
FROM nginx:alpine
COPY --from=build /app/dist /usr/share/nginx/html
COPY webserver/nginx/default.conf /etc/nginx/conf.d/default.conf
