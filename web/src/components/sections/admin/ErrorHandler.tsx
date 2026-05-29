import { useState, useEffect } from "react";
import { toast } from "sonner";
import {
    AlertDialog,
    AlertDialogAction,
    AlertDialogContent,
    AlertDialogDescription,
    AlertDialogFooter,
    AlertDialogHeader,
    AlertDialogTitle,
} from "@/components/ui/alert-dialog";
import { AlertTriangle } from "lucide-react";

// 1. A dictionary to translate URL error codes into human-readable messages
const ERROR_MESSAGES: Record<string, { title: string; description: string }> = {
    not_found: {
        title: "Item Not Found",
        description: "The requested resource or user does not exist or has been removed."
    },
    unauthorized: {
        title: "Access Denied",
        description: "You do not have the required permissions to perform this action."
    },
    session_expired: {
        title: "Session Expired",
        description: "For your security, your session has expired. Please log in again."
    },
    default: {
        title: "An Error Occurred",
        description: "Something went wrong while processing your request. Please try again."
    }
};

export function ErrorHandler() {
    const [modalError, setModalError] = useState<{ title: string; description: string } | null>(null);

    useEffect(() => {
        // Run this once when the component mounts
        const searchParams = new URLSearchParams(window.location.search);
        const errorCode = searchParams.get("error");
        const errorType = searchParams.get("errorType") || "toast"; // Default to toast if not specified

        if (errorCode) {
            // Get the message, or fallback to default if the code isn't in our dictionary
            const errorDetails = ERROR_MESSAGES[errorCode] || ERROR_MESSAGES.default;

            // Trigger the correct UI
            if (errorType === "modal") {
                setModalError(errorDetails);
            } else {
                toast.error(errorDetails.title, {
                    description: (<span className="text-secondary text-sm">
                        {errorDetails.description}
                    </span>)
                });
            }
            const newUrl = window.location.pathname;
            window.history.replaceState({}, document.title, newUrl);
        }
    }, []);

    // If there is no modal error, render nothing (the toast handles itself)
    if (!modalError) return null;

    return (
        <AlertDialog open={!!modalError} onOpenChange={() => setModalError(null)}>
            <AlertDialogContent>
                <AlertDialogHeader>
                    <AlertDialogTitle className="flex items-center gap-2 text-red-600">
                        <AlertTriangle className="w-5 h-5" />
                        {modalError.title}
                    </AlertDialogTitle>
                    <AlertDialogDescription className="text-slate-600">
                        {modalError.description}
                    </AlertDialogDescription>
                </AlertDialogHeader>
                <AlertDialogFooter>
                    <AlertDialogAction
                        onClick={() => setModalError(null)}
                        className="bg-slate-900 hover:bg-slate-800 text-white"
                    >
                        I Understand
                    </AlertDialogAction>
                </AlertDialogFooter>
            </AlertDialogContent>
        </AlertDialog>
    );
}