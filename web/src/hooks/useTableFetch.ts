import { useState, useEffect } from "react";
import { toast } from "sonner";



interface UseTableFetchProps {
    endpoint: string;
    filters: Record<string, string | number>;
}

export function useTableFetch<T>({ endpoint, filters }: UseTableFetchProps) {
    const [data, setData] = useState<T[]>([]);
    const [page, setPage] = useState(1);
    const [totalPages, setTotalPages] = useState(1);
    const [isLoading, setIsLoading] = useState(false);

    const filtersString = JSON.stringify(filters);

    useEffect(() => {
        setPage(1)
    }, [filtersString]);

    useEffect(() => {
        async function fetchData() {
            setIsLoading(true);
            try {

                const queryParams = new URLSearchParams({
                    page: page.toString(),
                    ...filters
                }).toString();

                const response = await fetch(`${endpoint}?${queryParams}`);
                if (response.ok) {
                    const data = await response.json();
                    setData(data.items || data);
                    setTotalPages(data.totalPages || 1);
                }
            } catch (error) {
                console.error("Fetch failed", error);
                toast.error("Failed to fetch data");
            } finally {
                setIsLoading(false);
            }
        }
        const delay = setTimeout(() => {
            fetchData();
        }, 300);
        return () => clearTimeout(delay);
    }, [page, filtersString]);

    return {
        data,
        page,
        totalPages,
        isLoading,
        setPage,
        setData
    }
}