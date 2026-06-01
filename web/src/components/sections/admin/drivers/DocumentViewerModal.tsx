import { useState } from "react";
import { Eye, X } from "lucide-react";

interface Props {
    docUrl: string;
    documentType: string;
}

export function DocumentViewerModal({ docUrl, documentType }: Props) {
    const [isOpen, setIsOpen] = useState(false);

    return (
        <>
            {/* The Trigger Button */}
            <button
                onClick={() => setIsOpen(true)}
                className="inline-flex items-center justify-end gap-1 text-yellow-600 font-semibold hover:text-yellow-700 transition-colors"
            >
                <Eye className="w-4 h-4" /> View
            </button>

            {/* The Modal Overlay */}
            {isOpen && (
                <div className="fixed inset-0 z-50 flex items-center justify-center p-4 bg-black/60 backdrop-blur-sm">
                    {/* The Modal Box */}
                    <div
                        className="bg-card rounded-xl shadow-2xl max-w-4xl w-full overflow-hidden animate-in fade-in zoom-in duration-200"
                    >
                        {/* Modal Header */}
                        <div className="flex justify-between items-center p-4 border-b">
                            <h3 className="font-bold text-lg text-foreground">{documentType}</h3>
                            <button
                                onClick={() => setIsOpen(false)}
                                className="p-1.5 text-muted-foreground hover:bg-muted rounded-md transition-colors"
                            >
                                <X className="w-5 h-5" />
                            </button>
                        </div>

                        {/* Modal Image Area */}
                        <div className="p-6 bg-muted flex justify-center items-center min-h-[300px]">
                            <img
                                src={docUrl}
                                alt={documentType}
                                className="max-h-[60vh] object-contain rounded-md shadow-sm border bg-card"
                            />
                        </div>
                    </div>
                </div>
            )}
        </>
    );
}