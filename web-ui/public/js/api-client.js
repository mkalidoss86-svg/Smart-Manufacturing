// API Client - handles all HTTP requests to backend services
class ApiClient {
    constructor(baseUrl) {
        this.baseUrl = baseUrl;
    }

    async fetch(endpoint, options = {}) {
        try {
            const url = `${this.baseUrl}${endpoint}`;
            const response = await fetch(url, {
                ...options,
                headers: {
                    'Content-Type': 'application/json',
                    ...options.headers
                }
            });

            if (!response.ok) {
                throw new Error(`HTTP ${response.status}: ${response.statusText}`);
            }

            return await response.json();
        } catch (error) {
            console.error(`API Error (${endpoint}):`, error);
            throw error;
        }
    }

    // Get system health status
    async getSystemHealth() {
        return this.fetch('/health');
    }

    // Get all production lines status
    async getProductionLines() {
        return this.fetch('/production-lines');
    }

    // Get production line details
    async getProductionLine(lineId) {
        return this.fetch(`/production-lines/${lineId}`);
    }

    // Get recent defects
    async getDefects(limit = 50) {
        return this.fetch(`/defects?limit=${limit}`);
    }

    // Get defects for a specific line
    async getDefectsByLine(lineId, limit = 50) {
        return this.fetch(`/defects?lineId=${lineId}&limit=${limit}`);
    }

    // Get anomalies
    async getAnomalies(limit = 50) {
        return this.fetch(`/anomalies?limit=${limit}`);
    }
}

// Export for use in other modules
window.ApiClient = ApiClient;
