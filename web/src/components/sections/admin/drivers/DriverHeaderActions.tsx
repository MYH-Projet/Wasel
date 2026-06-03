import { useState } from "react";
import { Mail, Ban, AlertTriangle } from "lucide-react";
import { toast } from "sonner";

interface Props {
    driverId: string;
    driverName: string;
}

export function DriverHeaderActions({ driverId, driverName }: Props) {
    // 1. Separate the states!
    const [showModal, setShowModal] = useState(false);
    const [isSubmitting, setIsSubmitting] = useState(false);
    const [suspendReason, setSuspendReason] = useState("");

    const handleSuspend = async () => {
        setIsSubmitting(true);
        try {
            // Call your BFF (Backend-For-Frontend) endpoint
            const response = await fetch(`/endpoint/drivers/`, {
                method: "PATCH",
                headers: { "Content-Type": "application/json" },
                body: JSON.stringify({
                    driverId: driverId,
                    action: "Suspended",
                    payload: { reason: suspendReason }
                }),
            });

            if (response.ok) {
                toast.success("Driver suspended successfully.");
                setTimeout(() => window.location.reload(), 1000);
            } else {
                toast.error("Error while suspending driver.");
                setIsSubmitting(false); // Only stop loading if it fails
            }
        } catch (error) {
            toast.error("Erreur serveur.");
            setIsSubmitting(false);
        }
    };

    const handleMessage = () => {
        toast.info("Message functionality is under development !");
    };

    return (
        <>
            <div className="flex gap-3">
                <button
                    onClick={handleMessage}
                    className="flex items-center gap-2 px-4 py-2 bg-secondary text-secondary-foreground font-medium rounded-lg hover:bg-slate-700 transition"
                >
                    <Mail className="w-4 h-4" /> Message
                </button>
            </div>
        </>
    );
}