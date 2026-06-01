import type { ReactNode } from "react";
import type { LucideIcon } from "lucide-react";
import {
    Card,
    CardContent,
    CardFooter,
    CardHeader,
    CardTitle
} from "@/components/ui/card";

interface MetricCardProps {
    title: string;
    value: string | number;
    icon?: LucideIcon;
    trend?: { value: string; isPositive: boolean; label: string };
    footer?: ReactNode;
    iconBgColor?: string;
    iconTextColor?: string;
}

export function MetricCard({ title, value, icon: Icon, trend, footer, iconBgColor = "bg-slate-100", iconTextColor = "text-slate-600" }: MetricCardProps) {
    return (
        <Card className="flex flex-col h-full shadow-sm">
            <CardHeader className="flex flex-row items-center justify-between space-y-0 pb-2">
                <CardTitle className="text-sm font-medium text-slate-500">
                    {title}
                </CardTitle>
                {Icon && (
                    <div className={`p-2 rounded-lg ${iconBgColor} ${iconTextColor}`}>
                        <Icon className="w-4 h-4" />
                    </div>
                )}
            </CardHeader>

            <CardContent>
                <div className="text-6xl font-bold text-slate-900">{value}</div>
                {(trend && footer) && (
                    <div className="flex items-center gap-2 mt-2">
                        <span className={`text-xs font-bold ${trend.isPositive ? 'text-green-600' : 'text-red-600'}`}>
                            {trend.isPositive ? '↑' : '↓'} {trend.value}
                        </span>
                        <span className="text-xs text-slate-500">{trend.label}</span>
                    </div>
                )}
            </CardContent>

            {(trend && !footer) && (
                <CardFooter className="mt-auto pt-0">
                    <div className="flex items-center gap-2 mt-2">
                        <span className={`text-xs font-bold ${trend.isPositive ? 'text-green-600' : 'text-red-600'}`}>
                            {trend.isPositive ? '↑' : '↓'} {trend.value}
                        </span>
                        <span className="text-xs text-slate-500">{trend.label}</span>
                    </div>
                </CardFooter>
            )}

            {footer && (
                <CardFooter className="mt-auto pt-0">
                    {footer}
                </CardFooter>
            )}
        </Card>
    );
}