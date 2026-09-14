// Keep in sync with BubuTrackerAPI-Go handler JSON shapes.
export interface UserProfile {
  id: string;
  email: string;
  firstName: string;
  lastName: string;
  markerColor: string;
  createdAt?: string;
}

export interface TrackedLocation {
  userId: string;
  email: string;
  firstName: string;
  lastName: string;
  markerColor: string;
  latitude: number | null;
  longitude: number | null;
  updatedAt: string | null;
}

export interface UpdateProfileRequest {
  firstName?: string;
  lastName?: string;
  markerColor?: string;
}
