const express = require('express');
const path = require('path');
const app = express();

// Environment configuration
const PORT = process.env.PORT || 3000;
const API_BASE_URL = process.env.API_BASE_URL || 'http://localhost:5000/api';
const WEBSOCKET_URL = process.env.WEBSOCKET_URL || 'ws://localhost:5000/ws';
const REFRESH_INTERVAL = process.env.REFRESH_INTERVAL || '5000';
const MAX_DEFECTS_DISPLAY = process.env.MAX_DEFECTS_DISPLAY || '50';
const MAX_NOTIFICATIONS = process.env.MAX_NOTIFICATIONS || '20';

// Serve static files
app.use(express.static(path.join(__dirname, 'public')));

// Inject environment variables
app.get('/env.js', (req, res) => {
    res.type('application/javascript');
    res.send(`
        window.ENV = {
            API_BASE_URL: '${API_BASE_URL}',
            WEBSOCKET_URL: '${WEBSOCKET_URL}',
            REFRESH_INTERVAL: '${REFRESH_INTERVAL}',
            MAX_DEFECTS_DISPLAY: '${MAX_DEFECTS_DISPLAY}',
            MAX_NOTIFICATIONS: '${MAX_NOTIFICATIONS}'
        };
    `);
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
app.listen(PORT, () => {
    console.log(`VisionFlow Web UI running on port ${PORT}`);
    console.log(`Environment configuration:`);
    console.log(`  API_BASE_URL: ${API_BASE_URL}`);
    console.log(`  WEBSOCKET_URL: ${WEBSOCKET_URL}`);
    console.log(`  REFRESH_INTERVAL: ${REFRESH_INTERVAL}ms`);
});

// Graceful shutdown
process.on('SIGTERM', () => {
    console.log('SIGTERM signal received: closing HTTP server');
    process.exit(0);
});

process.on('SIGINT', () => {
    console.log('SIGINT signal received: closing HTTP server');
    process.exit(0);
});
