// Configuration loaded from environment variables
// These will be injected at runtime via the server
window.APP_CONFIG = {
    API_BASE_URL: window.ENV?.API_BASE_URL || 'http://localhost:5000/api',
    SIMULATOR_API_URL: window.ENV?.SIMULATOR_API_URL || 'http://localhost:5200/api',
    WEBSOCKET_URL: window.ENV?.WEBSOCKET_URL || 'ws://localhost:5000/ws',
    REFRESH_INTERVAL: parseInt(window.ENV?.REFRESH_INTERVAL || '5000'),
    MAX_DEFECTS_DISPLAY: parseInt(window.ENV?.MAX_DEFECTS_DISPLAY || '50'),
    MAX_NOTIFICATIONS: parseInt(window.ENV?.MAX_NOTIFICATIONS || '20'),
    
    // Production line status thresholds
    DEFECT_RATE_THRESHOLD_WARNING: parseFloat(window.ENV?.DEFECT_RATE_THRESHOLD_WARNING || '5.0'),
    DEFECT_RATE_THRESHOLD_CRITICAL: parseFloat(window.ENV?.DEFECT_RATE_THRESHOLD_CRITICAL || '15.0')
};
