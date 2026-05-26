import { useState, useMemo } from "react";
import { Bar, BarChart, CartesianGrid, XAxis, Cell } from "recharts";
import { useChart } from "@/hooks/useChart";
import {
    Card,
    CardContent,
    CardDescription,
    CardHeader,
    CardTitle
} from "@/components/ui/card";
import {
    ChartContainer,
    ChartTooltip,
    ChartTooltipContent,
} from "@/components/ui/chart";
import type { ChartConfig } from "@/components/ui/chart"

// Define the Shadcn Chart configuration
const chartConfig = {
    volume: {
        label: "Deliveries",
        color: "hsl(var(--primary))", // Shadcn uses CSS variables
    },
} satisfies ChartConfig;

const ranges = [
    { label: "12H", value: "12h" },
    { label: "24H", value: "24h" },
    { label: "7D", value: "7d" },
    { label: "1M", value: "1m" },
    { label: "1Y", value: "1y" },
];

export function DeliveryVolumeChart() {
    const [timeFrame, setTimeFrame] = useState<string>("12h");

    const { chartData, isLoading } = useChart({
        endpiont: "/endpoint/dashboard/delivery-volume",
        timeFrame
    });

    // Normalize the dynamic backend data into a standard { label, volume } format
    const normalizedData = useMemo(() => {
        if (!chartData) return [];

        return chartData.map(item => ({
            // Grabs whichever time key the backend decided to send
            label: item.month || item.day || item.hour || item.time || "Unknown",
            // Handles the 'volum' typo you mentioned, just in case!
            volume: item.volume || item.volum || 0
        }));
    }, [chartData]);

    return (
        <Card className="shadow-sm flex flex-col h-full">
            <CardHeader className="flex flex-col sm:flex-row items-start sm:items-center justify-between pb-6 gap-4">
                <div>
                    <CardTitle className="text-lg font-bold text-slate-900">Delivery Volume</CardTitle>
                    <CardDescription>Number of deliveries completed.</CardDescription>
                </div>

                {/* Time Frame Filter Buttons */}
                <div className="flex bg-slate-100 p-1 rounded-lg">
                    {ranges.map((range) => (
                        <button
                            key={range.value}
                            onClick={() => setTimeFrame(range.value)}
                            disabled={isLoading}
                            className={`px-3 py-1 text-xs font-bold rounded-md transition-all ${timeFrame === range.value
                                ? "bg-white text-slate-900 shadow-sm"
                                : "text-slate-500 hover:text-slate-700"
                                } ${isLoading ? "opacity-50 cursor-not-allowed" : ""}`}
                        >
                            {range.label}
                        </button>
                    ))}
                </div>
            </CardHeader>

            <CardContent className="flex-grow min-h-[350px]">
                {isLoading && normalizedData.length === 0 ? (
                    <div className="w-full h-full flex items-center justify-center min-h-[300px]">
                        <div className="animate-pulse flex space-x-4 items-end h-[200px] w-full px-10">
                            {/* Skeleton bars */}
                            {[1, 2, 3, 4, 5, 6, 7].map(i => (
                                <div key={i} className="flex-1 bg-slate-100 rounded-t-md" style={{ height: `${Math.random() * 100}%` }}></div>
                            ))}
                        </div>
                    </div>
                ) : (
                    <ChartContainer config={chartConfig} className="min-h-[300px] w-full">
                        <BarChart data={normalizedData} margin={{ top: 10, right: 10, left: -20, bottom: 0 }}>
                            <CartesianGrid strokeDasharray="3 3" vertical={false} stroke="#e2e8f0" />

                            <XAxis
                                dataKey="label"
                                tickLine={false}
                                tickMargin={10}
                                axisLine={false}
                                tick={{ fontSize: 12, fill: '#64748b' }}
                            />

                            {/* The beautiful Shadcn Tooltip */}
                            <ChartTooltip
                                cursor={{ fill: '#f8f9fa' }}
                                content={<ChartTooltipContent />}
                            />

                            <Bar dataKey="volume" radius={[4, 4, 0, 0]}>
                                {/* Colors the last bar yellow, keeps the rest gray to match your UI mockup */}
                                {normalizedData.map((_, index) => (
                                    <Cell
                                        key={`cell-${index}`}
                                        fill={index === normalizedData.length - 1 ? "#eab308" : "#e2e8f0"}
                                    />
                                ))}
                            </Bar>
                        </BarChart>
                    </ChartContainer>
                )}
            </CardContent>
        </Card>
    );
}