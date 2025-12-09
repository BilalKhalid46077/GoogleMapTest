namespace GoogleMapTest;

using Microsoft.Maui.Controls.Maps;
using Microsoft.Maui.Maps;

public partial class MainPage : ContentPage
{
    private readonly LocationDatabase _database;

    bool trackingStatus = false;
    Polyline polyline = new Polyline
    {
        StrokeColor = Colors.Red.WithAlpha(0.3f),
        StrokeWidth = 20
    };

    List<Circle> heatMap = new List<Circle>();
    public MainPage(LocationDatabase database)
    {
        InitializeComponent();
        ShowCurrentLocation();
        _database = database;
        // Load Previous Tracking Data if any
        LoadHeatMap();
    }
    // Tracking Button Clicked
    void OnTrackingClicked(object sender, EventArgs e)
    {
        if (!trackingStatus)
        {
            clearMap();
            TrackingButton.Text = "Stop Tracking";
            StartLocationTracking();
        }
        else
        {
            TrackingButton.Text = "Start New Tracking";
            StopTracking();
        }
        trackingStatus = !trackingStatus;
    }

    // Clear Heatmap, Pins, polyline and Database
    private async void clearMap()
    {
        MyMap.Pins.Clear();
        MyMap.MapElements.Remove(polyline);
        foreach (var circle in heatMap)
        {
            MyMap.MapElements.Remove(circle);
        }

        polyline.Clear();
        heatMap.Clear();
        await _database.ClearTableAsync();
    }
    // Start Location Tracking
    private async void StartLocationTracking()
    {
        try
        {
            var request = new GeolocationListeningRequest(GeolocationAccuracy.Best);
            await Geolocation.StartListeningForegroundAsync(request);
            Geolocation.LocationChanged += OnLocationChanged;
        }
        catch (Exception ex)
        {
            // Handle exceptions (e.g., location services disabled, timeout, etc.)
            await DisplayAlertAsync("Error", $"Unable to get location: {ex.Message}", "OK");
        }
    }

    // Location Changed Event Handler
    private void OnLocationChanged(object? sender, GeolocationLocationChangedEventArgs e)
    {
        if (e.Location != null)
        {
            var newPin = new Pin
            {
                Label = "Current Location",
                Location = new Location(e.Location.Latitude, e.Location.Longitude),
                Type = PinType.Place
            };
            MyMap.Pins.Add(newPin);
            // Optionally, center the map on the new location
            MyMap.MoveToRegion(MapSpan.FromCenterAndRadius(
                new Location(e.Location.Latitude, e.Location.Longitude), Distance.FromMiles(0.1)));
            SaveLocation(e.Location);
        }
    }

    // Save Location to Sqlite Database
    private async void SaveLocation(Location location)
    {
        var locationData = new LocationData
        {
            Latitude = location.Latitude,
            Longitude = location.Longitude
        };
        await _database.SaveLocationAsync(locationData);
    }

    // Load Heat Map from Sqlite Database
    private async void LoadHeatMap()
    {
        var locationList = await _database.GetLocationsAsync();

        foreach (var locationData in locationList)
        {
            var location = new Location(locationData.Latitude, locationData.Longitude);
            var circle = new Circle
            {
                Center = location,
                Radius = Distance.FromMeters(200), // adjust radius to change visual spread
                StrokeColor = Colors.Transparent,
                FillColor = Colors.Blue.WithAlpha(0.3f),
                StrokeWidth = 0
            };
            heatMap.Add(circle);
            polyline.Geopath.Add(location);
            MyMap.MapElements.Add(circle);
        }

        // Add the Polyline to the map's MapElements
        MyMap.MapElements.Add(polyline);

        if (locationList.Count > 0)
        {
            var centerLocationData = locationList.First();
            var centerLocation = new Location(centerLocationData.Latitude, centerLocationData.Longitude);
            var mapSpan = MapSpan.FromCenterAndRadius(centerLocation, Distance.FromMiles(0.5));
            MyMap.MoveToRegion(mapSpan);
        }
    }
    // Show Current Location on Map
    private async void ShowCurrentLocation()
    {
        try
        {
            // Request location permission if not already granted
            var status = await Permissions.CheckStatusAsync<Permissions.LocationWhenInUse>();
            if (status != PermissionStatus.Granted)
            {
                status = await Permissions.RequestAsync<Permissions.LocationWhenInUse>();
            }

            if (status == PermissionStatus.Granted)
            {
                // Get the current location
                var request = new GeolocationRequest(GeolocationAccuracy.High, TimeSpan.FromSeconds(10));
                var location = await Geolocation.Default.GetLocationAsync(request);

                if (location != null)
                {
                    // Create a MapSpan to define the visible area around the location
                    var mapSpan = MapSpan.FromCenterAndRadius(
                        new Location(location.Latitude, location.Longitude),
                        Distance.FromKilometers(0.5)); // Adjust zoom level as needed

                    // Move the map to the current location
                    MyMap.MoveToRegion(MapSpan.FromCenterAndRadius(location, Distance.FromMiles(0.5)));
                }
            }
            else
            {
                // Handle case where location permission is denied
                await DisplayAlertAsync("Permission Denied", "Location permission is required to show your current location.", "OK");
            }
        }
        catch (Exception ex)
        {
            // Handle exceptions (e.g., location services disabled, timeout, etc.)
            await DisplayAlertAsync("Error", $"Unable to get location: {ex.Message}", "OK");
        }
    }
    // Stop Tracking on Page Disappearing
    protected override void OnDisappearing()
    {
        base.OnDisappearing();
        StopTracking();
    }
    // Stop Location Tracking
    private void StopTracking()
    {
        Geolocation.LocationChanged -= OnLocationChanged;
        Geolocation.StopListeningForeground();
    }
}
