# Stage 1: Build the React application using Node.js
FROM node:20-alpine AS build
WORKDIR /app

# Install dependencies separately to optimize Docker layer caching
COPY package*.json ./
RUN npm install

# Build the static site with the necessary API URL environment variable
COPY . .
ARG VITE_API_URL
ENV VITE_API_URL=$VITE_API_URL
RUN npm run build

# Stage 2: Serve the application using Nginx
FROM nginx:stable-alpine

# Copy the production build folder to the Nginx static file directory
COPY --from=build /app/dist /usr/share/nginx/html

# Apply custom Nginx configuration (e.g., for handling client-side routing)
COPY nginx.conf /etc/nginx/conf.d/default.conf

# Expose port 80 for public traffic
EXPOSE 80

# Run Nginx in the foreground to keep the container alive
CMD ["nginx", "-g", "daemon off;"]