import { useState, useEffect } from "react";
import { toast } from "sonner";



export function useChart({ endpiont, timeFrame }: { endpiont: string, timeFrame: string }) {
    const [isLoading, setIsLoading] = useState<boolean>(false);
    const [chartData, setChartData] = useState<Record<string, number>[]>([]);



    useEffect(() => {
        async function fetchChartData() {
            setIsLoading(true);
            try {
                const response = await fetch(`${endpiont}?timeFrame=${timeFrame}`);
                const data = await response.json();
                if (!response.ok) {
                    throw new Error(data.message || `Data is not available right now. Please try again later`);
                }
                setChartData(data);

            } catch (err: any) {
                console.log(err);
                toast.error(err);
            } finally {
                setIsLoading(false);
            }
        }
        fetchChartData();

        const intervalId = setInterval(fetchChartData, 120000);
        return () => clearInterval(intervalId);
    }, [timeFrame])

    return { chartData, isLoading }

}