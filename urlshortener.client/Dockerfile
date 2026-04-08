# Stage 1: Build the React/Vue application
FROM node:20 AS build

# Set working directory inside container
WORKDIR /app

# Copy package.json and package-lock.json first
# This helps Docker cache dependencies and speeds up rebuilds
COPY package*.json ./

# Install project dependencies
RUN npm install

# Copy all source code into the container
COPY . .

# Build the production-ready static files
RUN npm run build


# Stage 2: Serve the app using Nginx
FROM nginx:alpine

# Copy built files from the previous stage into Nginx's default public folder
# Note: Depending on your framework, output folder might be 'dist' or 'build'
COPY --from=build /app/dist /usr/share/nginx/html

# (Optional) Add custom Nginx config if needed for SPA routing
# Example: handle React Router / Vue Router (history mode)
# COPY nginx.conf /etc/nginx/conf.d/default.conf

# Expose port 80 for web traffic
EXPOSE 80

# Start Nginx in foreground mode
CMD ["nginx", "-g", "daemon off;"]