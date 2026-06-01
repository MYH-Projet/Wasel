import { useState } from "react";
import { Search, CalendarIcon, Filter } from "lucide-react";
import { useTableFetch } from "@/hooks/useTableFetch";
import { DataTable } from "@/components/ui/DataTable";
import { format } from "date-fns";
import type { DateRange } from "react-day-picker";
import { cn } from "@/lib/utils";
import { Button } from "@/components/ui/button";
import { Calendar } from "@/components/ui/calendar";
import {
    Popover,
    PopoverContent,
    PopoverTrigger,
} from "@/components/ui/popover";

interface Driver {
    id: string;
    fullName: string;
    email: string;
    phone: string;
    driverStatus: "APPROVED" | "REJECTED" | "PENDING_VERIFICATION" | "SUSPENDED";
    dossierStatus: string;
    registrationDate: string;
    missionCount: number;
}


export function AllDriversTable() {
    const [search, setSearch] = useState("");
    const [statusFilter, setStatusFilter] = useState("")
    const [date, setDate] = useState<DateRange | undefined>();


    const { data, isLoading, page, totalPages, setPage } = useTableFetch<Driver>({
        endpoint: "/endpoint/drivers/allDrivers",
        filters: {
            search,
            statusFilter,
            dateFrom: date?.from ? format(date.from, "yyyy-MM-dd") : "",
            dateTo: date?.to ? format(date.to, "yyyy-MM-dd") : ""
        }
    });
    const columns = [
        { header: "Nom complet", render: (row: Driver) => <span className="font-medium text-foreground">{row.fullName}</span> },
        {
            header: "Contact", render: (row: Driver) => (
                <div className="flex flex-col text-xs text-muted-foreground">
                    <span>{row.email}</span>
                    <span>{row.phone}</span>
                </div>
            )
        },
        {
            header: "Statut", render: (row: Driver) => (
                <span className={`px-2 py-1 ${row.driverStatus === "APPROVED" ? "bg-green-100 text-green-700" : row.driverStatus === "REJECTED" ? "bg-red-100 text-red-700" : row.driverStatus === "PENDING_VERIFICATION" ? "bg-yellow-100 text-yellow-700" : "bg-muted text-foreground"} rounded-full text-xs font-semibold`}>
                    {row.driverStatus === "APPROVED" ? "Approved" : row.driverStatus === "REJECTED" ? "Rejected" : row.driverStatus === "PENDING_VERIFICATION" ? "Pending verification" : "Suspended"}
                </span>
            )
        },
        {
            header: "Actions", alignRight: true, render: (row: Driver) => (
                <div className="flex gap-2 justify-end">
                    <a href={`/admin/drivers/${row.id}`} className="inline-flex items-center justify-center gap-2 px-3 py-1.5 bg-primary text-primary-foreground rounded-md text-xs font-semibold hover:bg-primary/90 transition-colors">
                        Details
                    </a>
                </div>
            )
        }
    ];


    return (
        <div className="bg-card rounded-xl shadow-sm border p-4 space-y-4">

            {/* Your Custom Filters Toolbar */}
            <div className="flex flex-col md:flex-row justify-between gap-4">
                <div className="relative w-full md:w-72">
                    <Search className="absolute left-2.5 top-2.5 h-4 w-4 text-muted-foreground" />
                    <input
                        type="text"
                        placeholder="Rechercher nom ou email..."
                        className="pl-9 pr-4 py-2 w-full border rounded-md text-sm"
                        value={search}
                        onChange={(e) => setSearch(e.target.value)}
                    />
                </div>

                <div className="flex items-center gap-2">
                    <Filter className="w-4 h-4 text-muted-foreground" />
                    <select
                        className="border rounded-md px-3 py-2 text-sm"
                        value={statusFilter}
                        onChange={(e) => setStatusFilter(e.target.value)}
                    >
                        <option value="">all statuses</option>
                        <option value="APPROVED">Approved</option>
                        <option value="SUSPENDED">Suspended</option>
                        <option value="PENDING_VERIFICATION">Pending verification</option>
                        <option value="REJECTED">Rejected</option>
                    </select>
                </div>
                {/* Dropdowns and Dates */}
                <div className="flex flex-wrap items-center gap-2">

                    <Popover>
                        <PopoverTrigger asChild>
                            <Button
                                id="date"
                                variant={"outline"}
                                className={cn(
                                    "w-[260px] justify-start text-left font-normal",
                                    !date && "text-muted-foreground"
                                )}
                            >
                                <CalendarIcon className="mr-2 h-4 w-4" />
                                {date?.from ? (
                                    date.to ? (
                                        <>
                                            {format(date.from, "LLL dd, y")} -{" "}
                                            {format(date.to, "LLL dd, y")}
                                        </>
                                    ) : (
                                        format(date.from, "LLL dd, y")
                                    )
                                ) : (
                                    <span>Sélectionner une période</span>
                                )}
                            </Button>
                        </PopoverTrigger>
                        <PopoverContent className="w-auto p-0" align="end">
                            <Calendar
                                initialFocus
                                mode="range"
                                defaultMonth={date?.from}
                                selected={date}
                                onSelect={setDate}
                                numberOfMonths={1}
                                className="[&_.rdp-cell]:w-[--cell-size] [&_.rdp-cell]:h-[--cell-size]"
                                style={{ "--cell-size": "40px" } as React.CSSProperties}
                            />
                        </PopoverContent>
                    </Popover>
                </div>
            </div>

            {/* The Generic Table receiving the data! */}
            <DataTable
                columns={columns}
                data={data}
                isLoading={isLoading}
                page={page}
                totalPages={totalPages}
                onPageChange={setPage}
            />
        </div>
    );
}