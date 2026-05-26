import type { APIRoute } from "astro";

export const GET: APIRoute = async (context) => {
    try {
        // ⏳ Simulate network delay for the loading state (e.g. skeleton bars)
        await new Promise((resolve) => setTimeout(resolve, 600));

        // Get the timeFrame from the query string (e.g., ?timeFrame=7d)
        const url = new URL(context.request.url);
        const timeFrame = url.searchParams.get("timeFrame") || "12h";

        let mockData: any[] = [];

        // Helper to generate a random volume between min and max
        const getRandomVolume = (min: number, max: number) =>
            Math.floor(Math.random() * (max - min + 1)) + min;

        // Generate different data shapes based on the timeframe
        switch (timeFrame) {
            case "12h":
                // 12 data points representing the last 12 hours
                for (let i = 1; i <= 12; i++) {
                    mockData.push({ hour: `${i}:00`, volume: getRandomVolume(10, 50) });
                }
                break;

            case "24h":
                // 24 data points representing the last 24 hours
                for (let i = 1; i <= 24; i++) {
                    mockData.push({ hour: `${i}:00`, volume: getRandomVolume(5, 40) });
                }
                break;

            case "7d":
                // 7 data points for days of the week
                const days = ["Mon", "Tue", "Wed", "Thu", "Fri", "Sat", "Sun"];
                mockData = days.map(day => ({
                    day: day,
                    volume: getRandomVolume(100, 300)
                }));
                break;

            case "1m":
                // 30 data points representing days in a month
                for (let i = 1; i <= 30; i++) {
                    mockData.push({ day: `Oct ${i}`, volume: getRandomVolume(120, 350) });
                }
                break;

            case "1y":
                // 12 data points representing months of the year
                const months = ["Jan", "Feb", "Mar", "Apr", "May", "Jun", "Jul", "Aug", "Sep", "Oct", "Nov", "Dec"];
                mockData = months.map(month => ({
                    month: month,
                    volume: getRandomVolume(1500, 4500)
                }));
                break;

            default:
                mockData = [{ time: "Unknown", volume: 0 }];
        }

        const data = JSON.stringify(mockData);
        console.log(data)

        return new Response(data, {
            status: 200,
            headers: {
                "Content-Type": "application/json",
                "Cache-Control": "no-store, max-age=0"
            },
        });
    } catch (error) {
        console.error("Mock Delivery Volume Endpoint Error:", error);
        return new Response(JSON.stringify({ message: "Internal server error" }), {
            status: 500,
            headers: { "Content-Type": "application/json" },
        });
    }
};