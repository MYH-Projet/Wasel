export interface Address {
    id: string;
    label: string;
    street: string;
    city: string;
    latitude: number;
    longitude: number;
}

export interface Parcel {
    weight: number;
    isFragile: boolean;
    description: string;
}

export interface Payment {
    amount: number;
    method?: string;
    paymentMethod?: string;
    status?: string;
    paymentStatus?: string;
}

export interface AssignedDriver {
    name: string;
    phone: string;
    vehicle?: string;
}

export interface StatusHistory {
    status?: string;
    deliveryStatus?: string;
    timestamp?: string;
    changedAt?: string;
    note?: string;
    comment?: string;
}

export interface DeliveryDetails {
    id: string;
    deliveryStatus: string;
    createdAt: string;
    pickupAddress: Address;
    deliveryAddress: Address;
    parcel: Parcel;
    payment: Payment;
    assignedDriver: AssignedDriver | null;
    statusHistory: StatusHistory[];
}
