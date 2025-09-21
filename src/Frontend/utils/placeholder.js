export const servers = [
    {
        id: 1,
        host: "ya.ru",
        ip: "121.10.12.2",
        status: "doesn't work",
        protocol: "HTTP",
        errorMessage: "Error",
        statusCode: 404,
        stats: {
            totalPings: int,
            avgResponseTimeMs: 300,
            successRate: 100,
            lastCheck: string
        }
    },

    {
        id: 2,
        host: "wiki.ru",
        ip: "121.10.12.2",
        status: "doesn't work",
        protocol: "HTTP",
        errorMessage: "Error",
        statusCode: 404,
        stats: {
            totalPings: int,
            avgResponseTimeMs: 300,
            successRate: 100,
            lastCheck: string
        }
    },    {
        id: 3,
        host: "ozon.ru",
        ip: "121.10.12.2",
        status: "doesn't work",
        protocol: "HTTP",
        errorMessage: "Error",
        statusCode: 404,
        stats: {
            totalPings: int,
            avgResponseTimeMs: 300,
            successRate: 100,
            lastCheck: string
        }
    },    {
        id: 4,
        host: "830.131.31.23",
        ip: "121.10.12.2",
        status: "doesn't work",
        protocol: "HTTP",
        errorMessage: "OK",
        statusCode: 200,
        stats: {
            totalPings: int,
            avgResponseTimeMs: 300,
            successRate: 100,
            lastCheck: string
        }
    },    {
        id: 5,
        host: "ya.ru",
        ip: "121.10.12.2",
        status: "doesn't work",
        protocol: "HTTP",
        errorMessage: "Error",
        statusCode: 404,
        stats: {
            totalPings: int,
            avgResponseTimeMs: 300,
            successRate: 100,
            lastCheck: string
        }
    },
]

export const logs = [
    {
        id: 1,
        timestamp: "12:30",
        responseTimeMs: 120,
        success: true,
        statusCode: 200,
        protocol: "HTTP",
        errorMessage: "success"
    },
    {
        id: 2,
        timestamp: "12:30",
        responseTimeMs: 300,
        success: false,
        statusCode: 200,
        protocol: "HTTP",
        errorMessage: "success"
    },
    {
        id: 3,
        timestamp: "12:30",
        responseTimeMs: 250,
        success: true,
        statusCode: 200,
        protocol: "HTTP",
        errorMessage: "success"
    },
    {
        id: 4,
        timestamp: "12:30",
        responseTimeMs: 1100,
        success: false,
        statusCode: 200,
        protocol: "HTTP",
        errorMessage: "success"
    },
    {
        id: 5,
        timestamp: "12:31",
        responseTimeMs: 1100,
        success: false,
        statusCode: 200,
        protocol: "HTTP",
        errorMessage: "success"
    },
    {
        id: 6,
        timestamp: "12:33",
        responseTimeMs: 1100,
        success: false,
        statusCode: 200,
        protocol: "HTTP",
        errorMessage: "success"
    },
    {
        id: 7,
        timestamp: "12:40",
        responseTimeMs: 1100,
        success: false,
        statusCode: 200,
        protocol: "HTTP",
        errorMessage: "success"
    },
]

export const serverStats = [
    {
        id: 1,
        host: "ya.ru",
        avgResponseTimeMs: 300,
    },
    {
        id: 2,
        host: "google.com",
        avgResponseTimeMs: 120,
    },
    {
        id: 3,
        host: "wiki.com",
        avgResponseTimeMs: 250,
    },
    {
        id: 4,
        host: "ya.ru",
        avgResponseTimeMs: 400,
    },
    {
        id: 5,
        host: "ya.ru",
        avgResponseTimeMs: 500,
    },
    {
        id: 6,
        host: "ya.ru",
        avgResponseTimeMs: 100,
    },
    {
        id: 7,
        host: "ya.ru",
        avgResponseTimeMs: 1000,
    },
]

export const stats = {
    totalServers: 15,
    upServers: 11,
    downServers: 4,
    totalIncidentsToday: 4,
}