FROM node:18-alpine

WORKDIR /app

# Copy package files
COPY package.json ./

# Install dependencies
# Note: SSL verification is temporarily disabled due to build environment constraints
# In production, use a properly configured npm registry with valid certificates
RUN npm config set strict-ssl false && npm install --omit=dev

# Copy application files
COPY . .

# Make healthcheck script executable
RUN chmod +x healthcheck.sh

# Expose port
EXPOSE 3000

# Health check
HEALTHCHECK --interval=30s --timeout=3s --start-period=5s --retries=3 \
    CMD /app/healthcheck.sh

# Run as non-root user
USER node

# Start the application
CMD ["node", "server.js"]
