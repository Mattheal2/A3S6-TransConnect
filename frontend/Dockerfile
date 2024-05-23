# Base image
FROM python:3.12 as build-stage

# Set working directory
WORKDIR /app
RUN pip install jinja2 minify_html
COPY . .

RUN python render_v2.py --js

# Use NGINX as the web server
FROM nginx:alpine as production-stage

# Copy the built files from the build stage to the nginx container
COPY --from=build-stage /app/public /usr/share/nginx/html

# Copy the NGINX config file to the container
COPY nginx.conf /etc/nginx/conf.d/default.conf

# Expose port 80 for HTTP traffic
EXPOSE 80

# Start NGINX when the container starts
CMD ["nginx", "-g", "daemon off;"]