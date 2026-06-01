import {
    Card,
    CardHeader,
    CardTitle,
    CardContent
} from "@/components/ui/card";

interface FleetStatusProps {
    onDelivery: number;
    available: number;
    inactive: number;
}

export function FleetStatus({ onDelivery, available, inactive }: FleetStatusProps) {
    const total = onDelivery + available + inactive;
    const getPercent = (value: number) => total === 0 ? 0 : Math.round((value / total) * 100);

    return (
        <Card className="shadow-sm">
            <CardHeader className="pb-4">
                <CardTitle className="text-sm font-medium text-slate-500">
                    Fleet Status
                </CardTitle>
            </CardHeader>

            <CardContent className="space-y-5">
                <div>
                    <div className="flex justify-between text-sm mb-1.5">
                        <span className="font-bold text-slate-700">On Delivery</span>
                        <span className="text-slate-500">{getPercent(onDelivery)}%</span>
                    </div>
                    <div className="w-full bg-slate-100 rounded-full h-2">
                        <div className="bg-blue-500 h-2 rounded-full" style={{ width: `${getPercent(onDelivery)}%` }}></div>
                    </div>
                </div>

                <div>
                    <div className="flex justify-between text-sm mb-1.5">
                        <span className="font-bold text-slate-700">Available</span>
                        <span className="text-slate-500">{getPercent(available)}%</span>
                    </div>
                    <div className="w-full bg-slate-100 rounded-full h-2">
                        <div className="bg-green-500 h-2 rounded-full" style={{ width: `${getPercent(available)}%` }}></div>
                    </div>
                </div>

                <div>
                    <div className="flex justify-between text-sm mb-1.5">
                        <span className="font-bold text-slate-700">Inactive/Break</span>
                        <span className="text-slate-500">{getPercent(inactive)}%</span>
                    </div>
                    <div className="w-full bg-slate-100 rounded-full h-2">
                        <div className="bg-slate-300 h-2 rounded-full" style={{ width: `${getPercent(inactive)}%` }}></div>
                    </div>
                </div>
            </CardContent>
        </Card>
    );
}