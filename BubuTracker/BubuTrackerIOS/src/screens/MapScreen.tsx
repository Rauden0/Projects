import * as Location from "expo-location";
import React, { useCallback, useEffect, useRef, useState } from "react";
import {
  ActionSheetIOS,
  ActivityIndicator,
  AppState,
  type AppStateStatus,
  Modal,
  ScrollView,
  StyleSheet,
  Text,
  TouchableOpacity,
  View,
} from "react-native";
import MapView, { Marker, PROVIDER_DEFAULT, Region } from "react-native-maps";
import { api } from "../api/client";
import AppDialog, { type DialogButton } from "../components/AppDialog";
import Avatar from "../components/Avatar";
import PersonListModal from "../components/PersonListModal";
import TrackSomeoneModal from "../components/TrackSomeoneModal";
import { colors } from "../theme";
import type { TrackedLocation, UserProfile } from "../types";
import { actionErrorMessage } from "../utils/errors";
import { reverseGeocode } from "../utils/geocode";
import { formatLastSeenAt } from "../utils/lastSeen";
import { getFreshProfile, setProfile } from "../state/profileStore";

const POLL_INTERVAL_MS = 30_000;
const LOCATION_DISTANCE_M = 40;
const FORCE_UPLOAD_INTERVAL_MS = 5 * 60_000;

interface Props {
  onLogout: () => void;
  onOpenProfile: () => void;
}

type DialogState = {
  title?: string;
  message?: string;
  buttons: DialogButton[];
};

export default function MapScreen({ onLogout, onOpenProfile }: Props) {
  const mapRef = useRef<MapView | null>(null);
  const lastUploadedRef = useRef<{ lat: number; lng: number; at: number } | null>(null);
  const [myProfile, setMyProfile] = useState<UserProfile | null>(null);
  const [myPosition, setMyPosition] = useState<Location.LocationObjectCoords | null>(null);
  const [trackedLocations, setTrackedLocations] = useState<TrackedLocation[]>([]);
  const [selectedPending, setSelectedPending] = useState<UserProfile | null>(null);
  const [appActive, setAppActive] = useState(AppState.currentState === "active");

  const [requestsVisible, setRequestsVisible] = useState(false);
  const [incomingRequests, setIncomingRequests] = useState<UserProfile[]>([]);
  const [trackSomeoneVisible, setTrackSomeoneVisible] = useState(false);
  const [followersVisible, setFollowersVisible] = useState(false);
  const [followers, setFollowers] = useState<UserProfile[]>([]);

  const [selectedPerson, setSelectedPerson] = useState<TrackedLocation | null>(null);
  const [locationName, setLocationName] = useState<string | null>(null);

  const [dialog, setDialog] = useState<DialogState | null>(null);

  const showError = useCallback((message: string) => {
    setDialog({
      message,
      buttons: [{ label: "OK" }],
    });
  }, []);

  const showInfo = useCallback((title: string, message?: string) => {
    setDialog({
      title,
      message,
      buttons: [{ label: "OK" }],
    });
  }, []);

  const loadMyProfile = useCallback(() => {
    const cachedProfile = getFreshProfile();
    if (cachedProfile) {
      setMyProfile(cachedProfile);
      return;
    }
    api
      .getMe()
      .then((profile) => {
        setProfile(profile);
        setMyProfile(profile);
      })
      .catch(() => undefined);
  }, []);

  const lastTrackedLocationsJsonRef = useRef<string | null>(null);
  const loadTrackedLocations = useCallback(() => {
    api
      .getTrackedLocations()
      .then((locations) => {
        // Skip the state update (and the marker/list re-render it triggers)
        // when this poll returned exactly what the last one did - the common
        // case, since positions only change every few minutes at most.
        const json = JSON.stringify(locations);
        if (json === lastTrackedLocationsJsonRef.current) {
          return;
        }
        lastTrackedLocationsJsonRef.current = json;
        setTrackedLocations(locations);
      })
      .catch(() => undefined);
  }, []);

  useEffect(() => {
    const onChange = (state: AppStateStatus) => setAppActive(state === "active");
    const sub = AppState.addEventListener("change", onChange);
    return () => sub.remove();
  }, []);

  useEffect(() => {
    if (!appActive) {
      return;
    }
    loadMyProfile();
    loadTrackedLocations();
    const interval = setInterval(loadTrackedLocations, POLL_INTERVAL_MS);
    return () => clearInterval(interval);
  }, [appActive, loadMyProfile, loadTrackedLocations]);

  useEffect(() => {
    let subscription: Location.LocationSubscription | null = null;
    (async () => {
      const { status } = await Location.requestForegroundPermissionsAsync();
      if (status !== "granted") {
        return;
      }
      subscription = await Location.watchPositionAsync(
        {
          accuracy: Location.Accuracy.Balanced,
          timeInterval: POLL_INTERVAL_MS,
          distanceInterval: LOCATION_DISTANCE_M,
        },
        (location) => {
          setMyPosition(location.coords);
          const { latitude, longitude } = location.coords;
          const prev = lastUploadedRef.current;
          const now = Date.now();
          const moved =
            !prev ||
            haversineMeters(prev.lat, prev.lng, latitude, longitude) >= LOCATION_DISTANCE_M;
          const stale = !prev || now - prev.at >= FORCE_UPLOAD_INTERVAL_MS;
          if (!moved && !stale) {
            return;
          }
          lastUploadedRef.current = { lat: latitude, lng: longitude, at: now };
          api.updateMyLocation(latitude, longitude).catch(() => undefined);
        }
      );
    })();
    return () => subscription?.remove();
  }, []);

  const centerOnPerson = (location: TrackedLocation) => {
    if (location.latitude == null || location.longitude == null) {
      showInfo("No location reported yet");
      return;
    }
    const region: Region = {
      latitude: location.latitude,
      longitude: location.longitude,
      latitudeDelta: 0.02,
      longitudeDelta: 0.02,
    };
    mapRef.current?.animateToRegion(region, 400);
  };

  const openPersonDetail = async (location: TrackedLocation) => {
    setSelectedPerson(location);
    setLocationName(null);
    if (location.updatedAt == null || location.latitude == null || location.longitude == null) {
      setLocationName("Unknown location");
      return;
    }
    setLocationName("Locating…");
    const placeName = await reverseGeocode(location.latitude, location.longitude);
    setLocationName(placeName ?? "Unknown location");
  };

  const formatLastSeen = (updatedAt: string | null): string =>
    formatLastSeenAt(updatedAt, Date.now());

  const openManageMenu = () => {
    ActionSheetIOS.showActionSheetWithOptions(
      {
        options: ["Track someone", "Requests", "Followers", "Cancel"],
        cancelButtonIndex: 3,
      },
      (index) => {
        if (index === 0) setTrackSomeoneVisible(true);
        if (index === 1) openRequests();
        if (index === 2) openFollowers();
      }
    );
  };

  const openRequests = () => {
    api
      .getIncomingRequests()
      .then((requests) => {
        setIncomingRequests(requests);
        setRequestsVisible(true);
      })
      .catch((error) => showError(actionErrorMessage(error)));
  };

  const openFollowers = () => {
    api
      .getFollowers()
      .then((people) => {
        setFollowers(people);
        setFollowersVisible(true);
      })
      .catch((error) => showError(actionErrorMessage(error)));
  };

  const acceptIncoming = (requester: UserProfile) => {
    api
      .acceptRequest(requester.id)
      .then(() => {
        setIncomingRequests((current) => current.filter((person) => person.id !== requester.id));
      })
      .catch((error) => showError(actionErrorMessage(error)));
  };

  const rejectIncoming = (requester: UserProfile) => {
    api
      .removeFollower(requester.id)
      .then(() => {
        setIncomingRequests((current) => current.filter((person) => person.id !== requester.id));
      })
      .catch((error) => showError(actionErrorMessage(error)));
  };

  const handleRevokeFollower = (follower: UserProfile) => {
    const name = [follower.firstName, follower.lastName].filter(Boolean).join(" ") || follower.email;
    setDialog({
      title: "Revoke access?",
      message: `Stop ${name} from seeing your location?`,
      buttons: [
        { label: "Cancel", style: "cancel" },
        {
          label: "Revoke",
          style: "destructive",
          onPress: () =>
            api
              .removeFollower(follower.id)
              .then(() => {
                setFollowersVisible(false);
                showInfo("Access revoked");
              })
              .catch((error) => showError(actionErrorMessage(error))),
        },
      ],
    });
  };

  const handleStopTracking = () => {
    if (!selectedPerson) return;
    const person = selectedPerson;
    const name = [person.firstName, person.lastName].filter(Boolean).join(" ") || person.email;
    setDialog({
      title: "Stop tracking?",
      message: `Stop tracking ${name}? This cancels the request if it's still pending.`,
      buttons: [
        { label: "Cancel", style: "cancel" },
        {
          label: "Stop tracking",
          style: "destructive",
          onPress: () =>
            api
              .removeTracking(person.userId)
              .then(() => {
                setSelectedPerson(null);
                loadTrackedLocations();
              })
              .catch((error) => showError(actionErrorMessage(error))),
        },
      ],
    });
  };

  const dialogButtons: DialogButton[] = dialog?.buttons ?? [];

  return (
    <View style={styles.container}>
      <MapView
        ref={mapRef}
        provider={PROVIDER_DEFAULT}
        style={StyleSheet.absoluteFill}
        initialRegion={{ latitude: 0, longitude: 0, latitudeDelta: 60, longitudeDelta: 60 }}
      >
        {myPosition && myProfile && (
          <Marker
            coordinate={{ latitude: myPosition.latitude, longitude: myPosition.longitude }}
            anchor={{ x: 0.5, y: 0.5 }}
          >
            <Avatar
              firstName={myProfile.firstName}
              lastName={myProfile.lastName}
              email={myProfile.email}
              color={myProfile.markerColor}
            />
          </Marker>
        )}
        {trackedLocations
          .filter((location) => location.latitude != null && location.longitude != null)
          .map((location) => (
            <Marker
              key={location.userId}
              coordinate={{ latitude: location.latitude as number, longitude: location.longitude as number }}
              anchor={{ x: 0.5, y: 0.5 }}
              onPress={() => openPersonDetail(location)}
            >
              <Avatar
                firstName={location.firstName}
                lastName={location.lastName}
                email={location.email}
                color={location.markerColor}
              />
            </Marker>
          ))}
      </MapView>

      <View style={styles.topBar}>
        <TouchableOpacity style={styles.iconButton} onPress={openManageMenu}>
          <Text style={styles.iconText}>⚙️</Text>
        </TouchableOpacity>
        <TouchableOpacity style={styles.iconButton} onPress={onOpenProfile}>
          <Text style={styles.iconText}>👤</Text>
        </TouchableOpacity>
      </View>
      <TouchableOpacity style={[styles.iconButton, styles.logoutButton]} onPress={onLogout}>
        <Text style={styles.iconText}>⎋</Text>
      </TouchableOpacity>

      {trackedLocations.length > 0 && (
        <ScrollView
          horizontal
          showsHorizontalScrollIndicator={false}
          style={styles.peopleList}
          contentContainerStyle={styles.peopleListContent}
        >
          {trackedLocations.map((location) => (
            <TouchableOpacity
              key={location.userId}
              style={styles.personItem}
              onPress={() => {
                centerOnPerson(location);
                openPersonDetail(location);
              }}
            >
              <Avatar
                firstName={location.firstName}
                lastName={location.lastName}
                email={location.email}
                color={location.markerColor}
                size={48}
              />
              <Text style={styles.personLabel} numberOfLines={1}>
                {location.firstName || location.email}
              </Text>
            </TouchableOpacity>
          ))}
        </ScrollView>
      )}

      <TrackSomeoneModal
        visible={trackSomeoneVisible}
        onClose={() => setTrackSomeoneVisible(false)}
        onError={showError}
        onPickPending={(person) => setSelectedPending(person)}
      />

      <Modal visible={selectedPending !== null} animationType="slide" transparent onRequestClose={() => setSelectedPending(null)}>
        <TouchableOpacity style={styles.backdrop} activeOpacity={1} onPress={() => setSelectedPending(null)}>
          <View style={styles.detailSheet}>
            {selectedPending && (
              <>
                <View style={styles.detailHeader}>
                  <Avatar
                    firstName={selectedPending.firstName}
                    lastName={selectedPending.lastName}
                    email={selectedPending.email}
                    color={selectedPending.markerColor}
                    size={40}
                  />
                  <Text style={styles.detailName}>
                    {[selectedPending.firstName, selectedPending.lastName].filter(Boolean).join(" ") ||
                      selectedPending.email}
                  </Text>
                </View>
                <Text style={styles.detailLabel}>Status</Text>
                <Text style={styles.detailValue}>Pending — waiting for them to accept</Text>
                <TouchableOpacity
                  style={styles.stopTrackingButton}
                  onPress={() => {
                    const person = selectedPending;
                    setSelectedPending(null);
                    setDialog({
                      title: "Cancel request?",
                      message: `Cancel your tracking request to ${
                        [person.firstName, person.lastName].filter(Boolean).join(" ") || person.email
                      }?`,
                      buttons: [
                        { label: "Keep waiting", style: "cancel" },
                        {
                          label: "Cancel request",
                          style: "destructive",
                          onPress: () =>
                            api
                              .removeTracking(person.id)
                              .then(loadTrackedLocations)
                              .catch((error) => showError(actionErrorMessage(error))),
                        },
                      ],
                    });
                  }}
                >
                  <Text style={styles.stopTrackingText}>Cancel request</Text>
                </TouchableOpacity>
              </>
            )}
          </View>
        </TouchableOpacity>
      </Modal>

      <Modal visible={selectedPerson !== null} animationType="slide" transparent onRequestClose={() => setSelectedPerson(null)}>
        <TouchableOpacity style={styles.backdrop} activeOpacity={1} onPress={() => setSelectedPerson(null)}>
          <View style={styles.detailSheet}>
            {selectedPerson && (
              <>
                <View style={styles.detailHeader}>
                  <Avatar
                    firstName={selectedPerson.firstName}
                    lastName={selectedPerson.lastName}
                    email={selectedPerson.email}
                    color={selectedPerson.markerColor}
                    size={40}
                  />
                  <Text style={styles.detailName}>
                    {[selectedPerson.firstName, selectedPerson.lastName].filter(Boolean).join(" ") ||
                      selectedPerson.email}
                  </Text>
                </View>
                <Text style={styles.detailLabel}>Last seen</Text>
                <Text style={styles.detailValue}>{formatLastSeen(selectedPerson.updatedAt)}</Text>
                <Text style={styles.detailLabel}>Location</Text>
                {locationName === "Locating…" ? (
                  <ActivityIndicator style={styles.detailLoading} />
                ) : (
                  <Text style={styles.detailValue}>{locationName}</Text>
                )}
                <TouchableOpacity style={styles.stopTrackingButton} onPress={handleStopTracking}>
                  <Text style={styles.stopTrackingText}>Stop tracking</Text>
                </TouchableOpacity>
              </>
            )}
          </View>
        </TouchableOpacity>
      </Modal>

      <PersonListModal
        visible={requestsVisible}
        title="Requests"
        people={incomingRequests}
        emptyText="No pending tracking requests"
        onAccept={acceptIncoming}
        onReject={rejectIncoming}
        onClose={() => setRequestsVisible(false)}
      />
      <PersonListModal
        visible={followersVisible}
        title="Followers"
        people={followers}
        emptyText="No one is tracking you yet"
        onPick={handleRevokeFollower}
        onClose={() => setFollowersVisible(false)}
      />

      <AppDialog
        visible={dialog !== null}
        title={dialog?.title}
        message={dialog?.message}
        buttons={dialogButtons}
        onRequestClose={() => setDialog(null)}
      />
    </View>
  );
}

const styles = StyleSheet.create({
  container: { flex: 1 },
  topBar: { position: "absolute", top: 60, left: 16, flexDirection: "row" },
  logoutButton: { position: "absolute", top: 60, right: 16 },
  iconButton: {
    width: 44,
    height: 44,
    borderRadius: 22,
    backgroundColor: colors.surface,
    borderWidth: 1,
    borderColor: colors.brandPrimary,
    alignItems: "center",
    justifyContent: "center",
    marginRight: 10,
  },
  iconText: { fontSize: 20 },
  peopleList: { position: "absolute", bottom: 30, left: 0, right: 0 },
  peopleListContent: { paddingHorizontal: 16 },
  personItem: { alignItems: "center", marginRight: 16 },
  personLabel: { marginTop: 4, fontSize: 12, color: colors.onSurface, maxWidth: 70 },
  backdrop: { flex: 1, backgroundColor: "rgba(0,0,0,0.4)", justifyContent: "flex-end" },
  detailSheet: {
    backgroundColor: colors.surface,
    borderTopLeftRadius: 20,
    borderTopRightRadius: 20,
    padding: 24,
  },
  detailHeader: { flexDirection: "row", alignItems: "center", marginBottom: 18 },
  detailName: { marginLeft: 14, fontSize: 18, fontWeight: "bold", color: colors.onSurface },
  detailLabel: { fontSize: 12, color: colors.onSurfaceVariant, marginTop: 14 },
  detailValue: { fontSize: 16, color: colors.onSurface, marginTop: 2 },
  detailLoading: { alignSelf: "flex-start", marginTop: 4 },
  stopTrackingButton: {
    marginTop: 20,
    borderWidth: 1,
    borderColor: colors.brandPrimary,
    borderRadius: 24,
    paddingVertical: 12,
    alignItems: "center",
  },
  stopTrackingText: { color: colors.brandPrimary, fontWeight: "600", fontSize: 15 },
});

function haversineMeters(lat1: number, lon1: number, lat2: number, lon2: number): number {
  const toRad = (deg: number) => (deg * Math.PI) / 180;
  const dLat = toRad(lat2 - lat1);
  const dLon = toRad(lon2 - lon1);
  const a =
    Math.sin(dLat / 2) ** 2 +
    Math.cos(toRad(lat1)) * Math.cos(toRad(lat2)) * Math.sin(dLon / 2) ** 2;
  return 2 * 6_371_000 * Math.asin(Math.sqrt(a));
}