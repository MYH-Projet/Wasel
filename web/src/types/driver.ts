export interface DriverVehicle {
    type: string;
    matricule: string;
    model: string;
    marque: string;
}

export interface DriverDocument {
    documentId: string;
    documentType: string;
    objectKey: string;
    status: string;
    rejectionReason?: string;
    uploadedAt: string;
    verifiedAt?: string;
}

export interface DriverDetails {
    driverId: string;
    userId: string;
    firstName: string;
    lastName: string;
    email: string;
    phone: string;
    cin?: string;
    permitNumber: string;
    driverStatus: string;
    createdAt: string;
    dossierId?: string;
    dossierStatus?: string;
    submissionDate?: string;
    verificationDate?: string;
    rejectionReason?: string;
    vehicle: DriverVehicle | null;
    documents: DriverDocument[];
    totalDeliveries: number;
    rating: number;
    completionRate: number;
}
