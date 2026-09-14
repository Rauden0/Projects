import { reverseGeocode } from "./geocode";

function mockFetchOnce(body: unknown) {
  globalThis.fetch = jest.fn().mockResolvedValue({ json: async () => body });
}

describe("reverseGeocode", () => {
  beforeEach(() => {
    jest.clearAllMocks();
  });

  it("shortens the display name to the first two comma-separated parts", async () => {
    mockFetchOnce({ display_name: "123 Main St, Springfield, Some County, USA" });

    const result = await reverseGeocode(1.23456, 4.56789);

    expect(result).toBe("123 Main St, Springfield");
  });

  it("returns null when the response has no display_name", async () => {
    mockFetchOnce({});

    expect(await reverseGeocode(1.111, 2.222)).toBeNull();
  });

  it("returns null instead of throwing when the request fails", async () => {
    globalThis.fetch = jest.fn().mockRejectedValue(new Error("network down"));

    expect(await reverseGeocode(1.111, 2.222)).toBeNull();
  });

  it("caches a successful lookup and does not call fetch again for a nearby point", async () => {
    mockFetchOnce({ display_name: "Cached Place, Region, Country" });
    await reverseGeocode(10.00001, 20.00001);

    (globalThis.fetch as jest.Mock).mockClear();
    // Rounds to the same ~11m grid cell (4 decimal places) as the first call.
    const result = await reverseGeocode(10.000012, 20.000009);

    expect(result).toBe("Cached Place, Region");
    expect(globalThis.fetch).not.toHaveBeenCalled();
  });

  it("does not cache a failed lookup, so the next call retries instead of staying stuck", async () => {
    globalThis.fetch = jest.fn().mockRejectedValue(new Error("network down"));
    await reverseGeocode(30.5, 40.5);

    mockFetchOnce({ display_name: "Recovered, Region, Country" });
    const result = await reverseGeocode(30.5, 40.5);

    expect(result).toBe("Recovered, Region");
    expect(globalThis.fetch).toHaveBeenCalledTimes(1);
  });

  it("treats far-apart coordinates as distinct cache entries", async () => {
    mockFetchOnce({ display_name: "Place One, Region, Country" });
    await reverseGeocode(1, 1);

    mockFetchOnce({ display_name: "Place Two, Region, Country" });
    const result = await reverseGeocode(50, 60);

    expect(result).toBe("Place Two, Region");
    expect(globalThis.fetch).toHaveBeenCalledTimes(1);
  });
});
