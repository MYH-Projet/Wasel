import { useState } from "react";
import { Search, Filter, CalendarIcon, Eye, Ban, AlertCircle } from "lucide-react";
import { useTableFetch } from "@/hooks/useTableFetch";
import { DataTable } from "@/components/ui/DataTable";
import { format } from "date-fns";
import type { DateRange } from "react-day-picker";
import { cn } from "@/lib/utils";
import { Button } from "@/components/ui/button";
import { Calendar } from "@/components/ui/calendar";
import { Popover, PopoverContent, PopoverTrigger } from "@/components/ui/popover";
import { toast } from "sonner";

interface Delivery {
    id: string;
    customer: { id: string; name: string };
    driver: { id: string; name: string } | null;
    status: "PENDING" | "ACCEPTED" | "PICKED_UP" | "IN_TRANSIT" | "DELIVERED" | "CANCELLED";
    createdAt: string;
    price: number;
}

export function AllDeliveriesTable() {
    const [search, setSearch] = useState("");
    const [statusFilter, setStatusFilter] = useState("");
    const [date, setDate] = useState<DateRange | undefined>();

    // Cancellation Modal State
    const [cancelModalData, setCancelModalData] = useState<{ isOpen: boolean; deliveryId: string | null }>({ isOpen: false, deliveryId: null });
    const [cancelReason, setCancelReason] = useState("");
    const [isCancelling, setIsCancelling] = useState(false);

    // Fetch Hook
    const { data, isLoading, page, totalPages, setPage, setData } = useTableFetch<Delivery>({
        endpoint: "/endpoint/deliveries",
        filters: {
            search,
            status: statusFilter,
            dateFrom: date?.from ? format(date.from, "yyyy-MM-dd") : "",
            dateTo: date?.to ? format(date.to, "yyyy-MM-dd") : ""
        }
    });

    const handleCancelDelivery = async () => {
        setIsCancelling(true);
        try {
            const response = await fetch(
                `/endpoint/deliveries`,
                {
                    method: "POST",
                    headers: {
                        "Content-Type": "application/json",
                    },
                    body: JSON.stringify({ id: cancelModalData.deliveryId, reason: cancelReason }),
                },
            );

            if (!response.ok) {
                const error = await response.json();
                toast.error(error.message || "Failed to cancel delivery.");
                setIsCancelling(false);
                return;
            }
            toast.success(`Delivery ${cancelModalData.deliveryId} cancelled.`);

            // Optimistically update the table data without a full reload
            setData((prevData) =>
                prevData.map((delivery) =>
                    delivery.id === cancelModalData.deliveryId
                        ? { ...delivery, status: "CANCELLED" }
                        : delivery,
                ),
            );

            setCancelModalData({ isOpen: false, deliveryId: null });
            setCancelReason("");
        } catch (error) {
            toast.error("Failed to cancel delivery.");
        } finally {
            setIsCancelling(false);
        }
    };

    const getStatusBadge = (status: string) => {
        const styles: Record<string, string> = {
            PENDING: "bg-muted text-muted-foreground",
            ACCEPTED: "bg-blue-100 text-blue-700",
            PICKED_UP: "bg-purple-100 text-purple-700",
            IN_TRANSIT: "bg-primary/20 text-foreground",
            DELIVERED: "bg-green-100 text-green-700",
            CANCELLED: "bg-destructive/20 text-destructive",
        };
        return <span className={`px-2 py-1 text-[11px] font-bold rounded-md uppercase tracking-wider ${styles[status] || styles.PENDING}`}>{status.replace('_', ' ')}</span>;
    };

    const columns = [
        { header: "Tracking ID", render: (row: Delivery) => <span className="font-bold text-foreground">{row.id.slice(0, 8) + "...." + row.id.slice(-3)}</span> },
        {
            header: "Customer",
            render: (row: Delivery) => <span className="font-medium text-foreground/90">{row.customer.name}</span>
        },
        {
            header: "Assigned Driver",
            render: (row: Delivery) => (
                <span className={row.driver ? "text-foreground/90" : "text-muted-foreground/80 italic"}>
                    {row.driver ? row.driver.name : "Unassigned"}
                </span>
            )
        },
        { header: "Status", render: (row: Delivery) => getStatusBadge(row.status) },
        {
            header: "Created At",
            render: (row: Delivery) => (
                <div className="flex flex-col text-xs text-muted-foreground">
                    <span>{format(new Date(row.createdAt), "MMM dd, yyyy")}</span>
                    <span>{format(new Date(row.createdAt), "HH:mm")}</span>
                </div>
            )
        },
        { header: "Price", render: (row: Delivery) => <span className="font-bold text-foreground">{row.price.toFixed(2)} MAD</span> },
        {
            header: "Actions", alignRight: true, render: (row: Delivery) => {
                const canCancel = row.status !== "DELIVERED" && row.status !== "CANCELLED";
                return (
                    <div className="flex gap-2 justify-end items-center">
                        <a href={`/admin/deliveries/${row.id}`} className="p-1.5 text-muted-foreground hover:text-foreground hover:bg-muted rounded-md transition-colors" title="View Details">
                            <Eye className="w-4 h-4" />
                        </a>
                        {canCancel && (
                            <button
                                onClick={() => setCancelModalData({ isOpen: true, deliveryId: row.id })}
                                className="p-1.5 text-destructive hover:text-destructive/90 hover:bg-destructive/10 rounded-md transition-colors"
                                title="Cancel Delivery"
                            >
                                <Ban className="w-4 h-4" />
                            </button>
                        )}
                    </div>
                )
            }
        }
    ];

    return (
        <div className="bg-card rounded-xl shadow-sm border border-border p-4 space-y-4">

            {/* Filters Toolbar */}
            <div className="flex flex-col xl:flex-row justify-between gap-4">
                <div className="relative w-full xl:w-80">
                    <Search className="absolute left-2.5 top-2.5 h-4 w-4 text-muted-foreground" />
                    <input
                        type="text"
                        placeholder="Search by ID or Customer..."
                        className="pl-9 pr-4 py-2 w-full border border-border rounded-md text-sm outline-none focus:border-ring focus:ring-1 focus:ring-ring bg-background text-foreground"
                        value={search}
                        onChange={(e) => setSearch(e.target.value)}
                    />
                </div>

                <div className="flex flex-wrap items-center gap-3">
                    <div className="flex items-center gap-2 bg-muted/50 border border-border rounded-md px-3 py-1 text-foreground">
                        <Filter className="w-4 h-4 text-muted-foreground" />
                        <select
                            className="bg-transparent border-none text-sm outline-none focus:ring-0 cursor-pointer text-foreground"
                            value={statusFilter}
                            onChange={(e) => setStatusFilter(e.target.value)}
                        >
                            <option value="" className="bg-card text-foreground">All Statuses</option>
                            <option value="PENDING" className="bg-card text-foreground">Pending</option>
                            <option value="ACCEPTED" className="bg-card text-foreground">Accepted</option>
                            <option value="PICKED_UP" className="bg-card text-foreground">Picked Up</option>
                            <option value="IN_TRANSIT" className="bg-card text-foreground">In Transit</option>
                            <option value="DELIVERED" className="bg-card text-foreground">Delivered</option>
                        </select>
                    </div>

                    <Popover>
                        <PopoverTrigger asChild>
                            <Button
                                variant={"outline"}
                                className={cn("w-[260px] justify-start text-left font-normal border-border bg-card text-foreground hover:bg-muted", !date && "text-muted-foreground")}
                            >
                                <CalendarIcon className="mr-2 h-4 w-4 text-muted-foreground" />
                                {date?.from ? (
                                    date.to ? <>{format(date.from, "LLL dd, y")} - {format(date.to, "LLL dd, y")}</> : format(date.from, "LLL dd, y")
                                ) : (
                                    <span>Select Date Range</span>
                                )}
                            </Button>
                        </PopoverTrigger>
                        <PopoverContent className="w-auto p-0 border-border bg-card" align="end">
                            <Calendar
                                initialFocus
                                mode="range"
                                defaultMonth={date?.from}
                                selected={date}
                                onSelect={setDate}
                                numberOfMonths={1}
                            />
                        </PopoverContent>
                    </Popover>
                </div>
            </div>

            {/* The Generic Table */}
            <DataTable columns={columns} data={data} isLoading={isLoading} page={page} totalPages={totalPages} onPageChange={setPage} />

            {/* Cancellation Modal (Rendered securely outside the table) */}
            {cancelModalData.isOpen && (
                <div className="fixed inset-0 z-50 flex items-center justify-center bg-black/50 backdrop-blur-sm">
                    <div className="bg-card p-6 rounded-xl shadow-xl border border-border w-full max-w-md animate-in fade-in zoom-in duration-200">
                        <h4 className="text-lg font-bold flex items-center gap-2 text-foreground">
                            <AlertCircle className="text-destructive w-5 h-5" /> Cancel Delivery
                        </h4>
                        <p className="text-sm text-muted-foreground mt-2">
                            Are you sure you want to cancel delivery <span className="font-bold text-foreground">{cancelModalData.deliveryId}</span>? This action cannot be undone and the customer will be notified.
                        </p>

                        <div className="mt-4">
                            <label className="text-xs font-bold text-foreground/85 uppercase tracking-wider mb-1 block">Reason for Cancellation</label>
                            <textarea
                                className="w-full border border-border rounded-md p-3 text-sm focus:ring-2 focus:ring-destructive focus:border-destructive outline-none bg-background text-foreground"
                                rows={3}
                                placeholder="E.g., Customer requested cancellation, address invalid..."
                                value={cancelReason}
                                onChange={(e) => setCancelReason(e.target.value)}
                            />
                        </div>

                        <div className="mt-6 flex justify-end gap-3">
                            <button
                                onClick={() => setCancelModalData({ isOpen: false, deliveryId: null })}
                                disabled={isCancelling}
                                className="px-4 py-2 text-sm font-medium text-muted-foreground hover:bg-muted rounded-md transition-colors"
                            >
                                Go Back
                            </button>
                            <button
                                onClick={handleCancelDelivery}
                                disabled={cancelReason.trim().length < 5 || isCancelling}
                                className="px-4 py-2 text-sm font-medium bg-destructive text-destructive-foreground rounded-md hover:bg-destructive/90 disabled:opacity-50 transition-colors"
                            >
                                {isCancelling ? "Processing..." : "Confirm Cancellation"}
                            </button>
                        </div>
                    </div>
                </div>
            )}
        </div>
    );
}