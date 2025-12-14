// Main Application Logic
class VisionFlowApp {
    constructor() {
        this.apiClient = new ApiClient(window.APP_CONFIG.API_BASE_URL);
        this.wsClient = new WebSocketClient(window.APP_CONFIG.WEBSOCKET_URL);
        this.notifications = [];
        this.refreshInterval = null;
    }

    init() {
        console.log('Initializing VisionFlow Dashboard...');
        
        // Set up WebSocket event listeners
        this.setupWebSocket();
        
        // Initial data load
        this.loadAllData();
        
        // Set up periodic refresh
        this.startPeriodicRefresh();
        
        // Handle visibility change to pause/resume updates
        document.addEventListener('visibilitychange', () => {
            if (document.hidden) {
                this.stopPeriodicRefresh();
            } else {
                this.startPeriodicRefresh();
            }
        });
    }

    setupWebSocket() {
        this.wsClient.on('connected', () => {
            this.updateConnectionStatus(true);
        });

        this.wsClient.on('disconnected', () => {
            this.updateConnectionStatus(false);
        });

        this.wsClient.on('error', (error) => {
            console.error('WebSocket error:', error);
            this.updateConnectionStatus(false);
        });

        this.wsClient.on('message', (data) => {
            this.handleRealtimeMessage(data);
        });

        // Attempt to connect
        this.wsClient.connect();
    }

    updateConnectionStatus(isConnected) {
        const indicator = document.getElementById('connectionStatus');
        const text = document.getElementById('connectionText');
        
        if (isConnected) {
            indicator.className = 'status-indicator connected';
            text.textContent = 'Connected';
        } else {
            indicator.className = 'status-indicator disconnected';
            text.textContent = 'Disconnected';
        }
    }

    handleRealtimeMessage(data) {
        console.log('Received real-time message:', data);

        // Add notification
        this.addNotification(data);

        // Update relevant sections based on message type
        if (data.type === 'defect' || data.type === 'anomaly') {
            this.loadDefects();
        }
        
        if (data.type === 'line-status') {
            this.loadProductionLines();
        }

        if (data.type === 'health-update') {
            this.loadSystemHealth();
        }
    }

    addNotification(data) {
        const notification = {
            time: new Date(),
            message: data.message || JSON.stringify(data),
            type: data.severity || data.type || 'info'
        };

        this.notifications.unshift(notification);
        
        // Limit notifications
        if (this.notifications.length > window.APP_CONFIG.MAX_NOTIFICATIONS) {
            this.notifications = this.notifications.slice(0, window.APP_CONFIG.MAX_NOTIFICATIONS);
        }

        this.renderNotifications();
    }

    renderNotifications() {
        const container = document.getElementById('notifications');
        
        if (this.notifications.length === 0) {
            container.innerHTML = `
                <div class="notification info">
                    <span class="notification-time">--:--:--</span>
                    <span class="notification-message">No notifications yet</span>
                </div>
            `;
            return;
        }

        container.innerHTML = this.notifications.map(notif => `
            <div class="notification ${notif.type}">
                <span class="notification-time">${this.formatTime(notif.time)}</span>
                <span class="notification-message">${this.escapeHtml(notif.message)}</span>
            </div>
        `).join('');
    }

    async loadAllData() {
        await Promise.all([
            this.loadSystemHealth(),
            this.loadProductionLines(),
            this.loadDefects()
        ]);
    }

    async loadSystemHealth() {
        try {
            const health = await this.apiClient.getSystemHealth();
            this.renderSystemHealth(health);
        } catch (error) {
            console.error('Failed to load system health:', error);
            this.renderSystemHealthError();
        }
    }

    renderSystemHealth(health) {
        const container = document.getElementById('systemHealth');
        
        if (!health || !health.services) {
            this.renderSystemHealthError();
            return;
        }

        container.innerHTML = health.services.map(service => `
            <div class="health-item">
                <span class="service-name">${this.escapeHtml(service.name)}</span>
                <span class="health-status ${service.status.toLowerCase()}">${service.status}</span>
            </div>
        `).join('');
    }

    renderSystemHealthError() {
        const container = document.getElementById('systemHealth');
        container.innerHTML = `
            <div class="health-item">
                <span class="service-name">Unable to load health data</span>
                <span class="health-status">-</span>
            </div>
        `;
    }

    async loadProductionLines() {
        try {
            const lines = await this.apiClient.getProductionLines();
            this.renderProductionLines(lines);
        } catch (error) {
            console.error('Failed to load production lines:', error);
            this.renderProductionLinesError();
        }
    }

    renderProductionLines(lines) {
        const container = document.getElementById('productionLines');
        
        if (!lines || lines.length === 0) {
            container.innerHTML = `
                <div class="line-card">
                    <div class="line-header">
                        <h3>No production lines available</h3>
                    </div>
                </div>
            `;
            return;
        }

        container.innerHTML = lines.map(line => `
            <div class="line-card">
                <div class="line-header">
                    <h3>${this.escapeHtml(line.name || line.id)}</h3>
                    <span class="status-badge ${line.status.toLowerCase()}">${line.status}</span>
                </div>
                <div class="line-metrics">
                    <div class="metric">
                        <span class="metric-label">Quality Score</span>
                        <span class="metric-value">${line.qualityScore ? line.qualityScore.toFixed(1) + '%' : '-'}</span>
                    </div>
                    <div class="metric">
                        <span class="metric-label">Defect Rate</span>
                        <span class="metric-value">${line.defectRate ? line.defectRate.toFixed(2) + '%' : '-'}</span>
                    </div>
                    <div class="metric">
                        <span class="metric-label">Units/Hour</span>
                        <span class="metric-value">${line.throughput || '-'}</span>
                    </div>
                </div>
            </div>
        `).join('');
    }

    renderProductionLinesError() {
        const container = document.getElementById('productionLines');
        container.innerHTML = `
            <div class="line-card">
                <div class="line-header">
                    <h3>Unable to load production lines</h3>
                    <span class="status-badge">-</span>
                </div>
            </div>
        `;
    }

    async loadDefects() {
        try {
            const defects = await this.apiClient.getDefects(window.APP_CONFIG.MAX_DEFECTS_DISPLAY);
            this.renderDefects(defects);
        } catch (error) {
            console.error('Failed to load defects:', error);
            this.renderDefectsError();
        }
    }

    renderDefects(defects) {
        const tbody = document.querySelector('#defectsTable tbody');
        
        if (!defects || defects.length === 0) {
            tbody.innerHTML = `
                <tr>
                    <td colspan="5" class="loading">No defects recorded</td>
                </tr>
            `;
            return;
        }

        tbody.innerHTML = defects.map(defect => `
            <tr>
                <td>${this.formatDateTime(defect.timestamp)}</td>
                <td>${this.escapeHtml(defect.lineId || defect.line)}</td>
                <td>${this.escapeHtml(defect.type)}</td>
                <td><span class="severity ${defect.severity.toLowerCase()}">${defect.severity}</span></td>
                <td>${this.escapeHtml(defect.description || '-')}</td>
            </tr>
        `).join('');
    }

    renderDefectsError() {
        const tbody = document.querySelector('#defectsTable tbody');
        tbody.innerHTML = `
            <tr>
                <td colspan="5" class="loading">Unable to load defects data</td>
            </tr>
        `;
    }

    startPeriodicRefresh() {
        if (this.refreshInterval) {
            return;
        }

        this.refreshInterval = setInterval(() => {
            this.loadAllData();
        }, window.APP_CONFIG.REFRESH_INTERVAL);
    }

    stopPeriodicRefresh() {
        if (this.refreshInterval) {
            clearInterval(this.refreshInterval);
            this.refreshInterval = null;
        }
    }

    formatTime(date) {
        return new Date(date).toLocaleTimeString();
    }

    formatDateTime(date) {
        const d = new Date(date);
        return `${d.toLocaleDateString()} ${d.toLocaleTimeString()}`;
    }

    escapeHtml(text) {
        const div = document.createElement('div');
        div.textContent = text;
        return div.innerHTML;
    }
}

// Initialize the app when DOM is ready
if (document.readyState === 'loading') {
    document.addEventListener('DOMContentLoaded', () => {
        window.app = new VisionFlowApp();
        window.app.init();
    });
} else {
    window.app = new VisionFlowApp();
    window.app.init();
}
