# Stage 1: Build React application using Node.js image
FROM node:20-alpine AS build
WORKDIR /app

# Copy package files and install dependencies to leverage Docker cache
COPY package*.json ./
RUN npm install

# Copy source code and build the production-ready application
COPY . .
RUN npm run build

# Stage 2: Serve the compiled static files with Nginx
FROM nginx:stable-alpine

# Copy build output from the build stage to Nginx default directory
COPY --from=build /app/dist /usr/share/nginx/html

# Copy custom nginx configuration for proxying
COPY nginx.conf /etc/nginx/conf.d/default.conf

# Expose port 80 for web traffic access
EXPOSE 80

# Start Nginx server in the foreground
CMD ["nginx", "-g", "daemon off;"]