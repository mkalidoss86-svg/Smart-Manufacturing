const express = require('express');
const rateLimit = require('express-rate-limit');
const path = require('path');
const app = express();

// Environment configuration
const PORT = process.env.PORT || 3000;
const API_BASE_URL = process.env.API_BASE_URL || 'http://localhost:5000/api';
const WEBSOCKET_URL = process.env.WEBSOCKET_URL || 'ws://localhost:5000/ws';
const REFRESH_INTERVAL = process.env.REFRESH_INTERVAL || '5000';
const MAX_DEFECTS_DISPLAY = process.env.MAX_DEFECTS_DISPLAY || '50';
const MAX_NOTIFICATIONS = process.env.MAX_NOTIFICATIONS || '20';

// Rate limiting configuration
const limiter = rateLimit({
    windowMs: 15 * 60 * 1000, // 15 minutes
    max: 1000, // Limit each IP to 1000 requests per windowMs
    standardHeaders: true,
    legacyHeaders: false,
    message: 'Too many requests from this IP, please try again later.'
});

// Apply rate limiting to all routes
app.use(limiter);

// Serve static files
app.use(express.static(path.join(__dirname, 'public')));

// Inject environment variables
app.get('/env.js', (req, res) => {
    res.type('application/javascript');
    const config = {
        API_BASE_URL,
        WEBSOCKET_URL,
        REFRESH_INTERVAL,
        MAX_DEFECTS_DISPLAY,
        MAX_NOTIFICATIONS
    };
    res.send(`window.ENV = ${JSON.stringify(config)};`);
});

// Health check endpoint
app.get('/health', (req, res) => {
    res.json({ 
        status: 'healthy',
        service: 'web-ui',
        timestamp: new Date().toISOString()
    });
});

// Serve index.html for all other routes (SPA support)
app.get('*', (req, res) => {
    res.sendFile(path.join(__dirname, 'public', 'index.html'));
});

// Start server
const server = app.listen(PORT, () => {
    console.log(`VisionFlow Web UI running on port ${PORT}`);
    console.log(`Environment configuration:`);
    console.log(`  API_BASE_URL: ${API_BASE_URL}`);
    console.log(`  WEBSOCKET_URL: ${WEBSOCKET_URL}`);
    console.log(`  REFRESH_INTERVAL: ${REFRESH_INTERVAL}ms`);
});

// Graceful shutdown
const gracefulShutdown = (signal) => {
    console.log(`${signal} signal received: closing HTTP server`);
    server.close(() => {
        console.log('HTTP server closed');
        process.exit(0);
    });
    
    // Force shutdown after 10 seconds
    setTimeout(() => {
        console.error('Forced shutdown after timeout');
        process.exit(1);
    }, 10000);
};

process.on('SIGTERM', () => gracefulShutdown('SIGTERM'));
process.on('SIGINT', () => gracefulShutdown('SIGINT'));
