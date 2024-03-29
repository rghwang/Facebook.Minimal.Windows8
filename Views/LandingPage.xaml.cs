﻿using Facebook.Client;
using Facebook.Minimal.Windows8.ViewModel;
using System;
using System.Collections.Generic;
using System.Dynamic;
using System.IO;
using System.Linq;
using System.Threading;
using Windows.Devices.Geolocation;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Popups;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;
using Windows.UI.Xaml.Navigation;

// 기본 페이지 항목 템플릿에 대한 설명은 http://go.microsoft.com/fwlink/?LinkId=234237에 나와 있습니다.

namespace Facebook.Minimal.Windows8.Views
{
    /// <summary>
    /// 대부분의 응용 프로그램에 공통되는 특성을 제공하는 기본 페이지입니다.
    /// </summary>
    public sealed partial class LandingPage : Facebook.Minimal.Windows8.Common.LayoutAwarePage
    {
        public LandingPage()
        {
            this.InitializeComponent();
            LoadUserInfo();
        }

        /// <summary>
        /// 탐색 중 전달된 콘텐츠로 페이지를 채웁니다. 이전 세션의 페이지를
        /// 다시 만들 때 저장된 상태도 제공됩니다.
        /// </summary>
        /// <param name="navigationParameter">이 페이지가 처음 요청될 때
        /// <see cref="Frame.Navigate(Type, Object)"/>에 전달된 매개 변수 값입니다.
        /// </param>
        /// <param name="pageState">이전 세션 동안 이 페이지에 유지된
        /// 사전 상태입니다. 페이지를 처음 방문할 때는 이 값이 null입니다.</param>
        protected override void LoadState(Object navigationParameter, Dictionary<String, Object> pageState)
        {
        }

        /// <summary>
        /// 응용 프로그램이 일시 중지되거나 탐색 캐시에서 페이지가 삭제된 경우
        /// 이 페이지와 관련된 상태를 유지합니다. 값은
        /// <see cref="SuspensionManager.SessionState"/>의 serialization 요구 사항을 만족해야 합니다.
        /// </summary>
        /// <param name="pageState">serializable 상태로 채워질 빈 사전입니다.</param>
        protected override void SaveState(Dictionary<String, Object> pageState)
        {
        }
        private async void LoadUserInfo()
        {
            FacebookClient fb = new FacebookClient(App.AccessToken);

            dynamic parameters = new ExpandoObject();
            parameters.access_token = App.AccessToken;
            parameters.fields = "name";

            dynamic result = await fb.GetTaskAsync("me", parameters);

            string profilePictureUrl = string.Format("https://graph.facebook.com/{0}/picture?type={1}&access_token={2}", App.FacebookId, "large", fb.AccessToken);

            this.MyImage.Source = new BitmapImage(new Uri(profilePictureUrl));
            this.MyName.Text = result.name;
        }
        async private void selectFriendsTextBox_Tapped(object sender, TappedRoutedEventArgs evtArgs)
        {
            FacebookClient fb = new FacebookClient(App.AccessToken);

            dynamic friendsTaskResult = await fb.GetTaskAsync("/me/friends");

            var result = (IDictionary<string, object>)friendsTaskResult;
            var data = (IEnumerable<object>)result["data"];
            foreach (var item in data)
            {
                var friend = (IDictionary<string, object>)item;
                if( ((string)friend["name"]).Contains("Hwang") ) FacebookData.Friends.Add(new Friend { Name = (string)friend["name"], id = (string)friend["id"], PictureUri = new Uri(string.Format("https://graph.facebook.com/{0}/picture?type={1}&access_token={2}", (string)friend["id"], "square", App.AccessToken)) });
            }
            Frame.Navigate(typeof(FriendSelector));
        }
        async private void selectRestaurantTextBox_Tapped(object sender, TappedRoutedEventArgs e)
        {
            Geolocator _geolocator = new Geolocator();
            CancellationTokenSource _cts = new CancellationTokenSource();
            CancellationToken token = _cts.Token;

            Geoposition pos = null;

            double latitude = 47.627903;
            double longitude = -122.143185;

            try
            {
                pos = await _geolocator.GetGeopositionAsync(new TimeSpan(48, 0, 0), new TimeSpan(0, 0, 0, 0, 100)).AsTask(token);
            }
            catch (Exception )
            {
            }

            if (pos != null)
            {
                latitude = pos.Coordinate.Latitude;
                longitude = pos.Coordinate.Longitude;
            }

            FacebookClient fb = new FacebookClient(App.AccessToken);
            dynamic restaurantsTaskResult = await fb.GetTaskAsync("/search", new { q = "restaurant", type = "place", center = latitude.ToString() + "," + longitude.ToString(), distance = "1000" });
            var result = (IDictionary<string, object>)restaurantsTaskResult;
            var data = (IEnumerable<object>)result["data"];

            foreach (var item in data)
	        {
		        var restaurant = (IDictionary<string, object>)item;
                var location = (IDictionary<string,object>)restaurant["location"];
                FacebookData.Locations.Add(new Location
                {
                    Street = location.ContainsKey("street") ? (string)location["street"] : String.Empty,
                    City = location.ContainsKey("city") ? (string)location["city"] : String.Empty,
                    State = location.ContainsKey("state") ? (string)location["state"] : String.Empty,
                    Country = location.ContainsKey("country") ? (string)location["country"] : String.Empty,
                    Zip = location.ContainsKey("zip") ? (string)location["zip"] : String.Empty,
                    Latitude = location.ContainsKey("latitude") ? ((double)location["latitude"]).ToString() : String.Empty,
                    Longitude = location.ContainsKey("longitude") ? ((double)location["longitude"]).ToString() : String.Empty,

                    // these properties are at the top level in the object
                    Category = restaurant.ContainsKey("category") ? (string)restaurant["category"] : String.Empty,
                    Name = restaurant.ContainsKey("name") ? (string)restaurant["name"] : String.Empty,
                    Id = restaurant.ContainsKey("id") ? (string)restaurant["id"] : String.Empty,
                    PictureUri = new Uri(string.Format("https://graph.facebook.com/{0}/picture?type={1}&access_token={2}", (string)restaurant["id"], "square", App.AccessToken))
                });
	        }
            Frame.Navigate(typeof(Restaurants));
        }
        private void selectMealTextBox_Tapped(object sender, Windows.UI.Xaml.Input.TappedRoutedEventArgs e)
        {
            Frame.Navigate(typeof(MealSelector));
        }
        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            if (FacebookData.SelectedFriends.Count > 0)
            {
                if (FacebookData.SelectedFriends.Count > 1)
                {
                    this.selectFriendsTextBox.Text = String.Format("with {0} and {1} others", FacebookData.SelectedFriends[0].Name, FacebookData.SelectedFriends.Count - 1);
                }
                else
                {
                    this.selectFriendsTextBox.Text = "with " + FacebookData.SelectedFriends[0].Name;
                }
            }
            else
            {
                this.selectFriendsTextBox.Text = "Select Friends";
            }
            if (FacebookData.IsRestaurantSelected)
            {
                this.selectRestaurantTextBox.Text = FacebookData.SelectedRestaurant.Name;
            }
            if (!String.IsNullOrEmpty(FacebookData.SelectedMeal.Name))
            {
                this.selectMealTextBox.Text = FacebookData.SelectedMeal.Name;
            }
        }

        async private void PostButtonAppbar_Tapped(object sender, TappedRoutedEventArgs e)
        {
            if (FacebookData.SelectedFriends.Count < 1
       || FacebookData.SelectedMeal.Name == String.Empty
       || FacebookData.IsRestaurantSelected == false)
            {
                MessageDialog errorMessageDialog = new MessageDialog("Please select friends, a place to eat and something you ate before attempting to share!");
                await errorMessageDialog.ShowAsync();
                return;
            }

            FacebookSession session = await App.FacebookSessionClient.LoginAsync("publish_stream");
            if (session == null)
            {
                MessageDialog dialog = new MessageDialog("Error while getting publishing permissions. Please try again.");
                await dialog.ShowAsync();
                return;
            }

            // refresh your access token to contain the publish permissions
            App.AccessToken = session.AccessToken;

            FacebookClient fb = new FacebookClient(App.AccessToken);

            try
            {
                dynamic fbPostTaskResult = await fb.PostTaskAsync(String.Format("/me/{0}:eat", Constants.FacebookAppGraphAction), new { meal = FacebookData.SelectedMeal.MealUri, tags = FacebookData.SelectedFriends[0].id, place = FacebookData.SelectedRestaurant.Id });
                var result = (IDictionary<string, object>)fbPostTaskResult;

                MessageDialog successMessageDialog = new MessageDialog("Posted Open Graph Action, id: " + (string)result["id"]);
                await successMessageDialog.ShowAsync();

                // reset the selections after the post action has successfully concluded
                this.selectFriendsTextBox.Text = "Select Friends";
                this.selectMealTextBox.Text = "Select One";
                this.selectRestaurantTextBox.Text = "Select One";
            }
            catch (Exception ex)
            {
                MessageDialog exceptionMessageDialog = new MessageDialog("Exception during post: " + ex.Message);
                exceptionMessageDialog.ShowAsync();
            }

        }
    }
}
