using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using Foundation;
using HealthKit;
using UIKit;

namespace Fit
{
    public partial class ProfileViewController : UITableViewController, IHealthStore
    {
        NSNumberFormatter numberFormatter;

        public HKHealthStore HealthStore { get; set; }

        public String Host { get; set; }

        NSSet DataTypesToWrite
        {
            get
            {
                return NSSet.MakeNSObjectSet<HKObjectType>(new HKObjectType[] {
					HKQuantityType.GetQuantityType (HKQuantityTypeIdentifierKey.DietaryEnergyConsumed),
					HKQuantityType.GetQuantityType (HKQuantityTypeIdentifierKey.ActiveEnergyBurned),
					HKQuantityType.GetQuantityType (HKQuantityTypeIdentifierKey.Height),
					HKQuantityType.GetQuantityType (HKQuantityTypeIdentifierKey.BodyMass)
				});
            }
        }

        NSSet DataTypesToRead
        {
            get
            {
                return NSSet.MakeNSObjectSet<HKObjectType>(new HKObjectType[] {
                    HKQuantityType.GetQuantityType (HKQuantityTypeIdentifierKey.LeanBodyMass),
                    HKQuantityType.GetQuantityType (HKQuantityTypeIdentifierKey.BodyMass),
                    HKQuantityType.GetQuantityType(HKQuantityTypeIdentifierKey.BodyMassIndex),
                    HKQuantityType.GetQuantityType(HKQuantityTypeIdentifierKey.BodyFatPercentage),
                    HKQuantityType.GetQuantityType (HKQuantityTypeIdentifierKey.Height),

                    HKQuantityType.GetQuantityType(HKQuantityTypeIdentifierKey.BloodPressureDiastolic),
                    HKQuantityType.GetQuantityType(HKQuantityTypeIdentifierKey.BloodPressureSystolic),
                    HKQuantityType.GetQuantityType(HKQuantityTypeIdentifierKey.HeartRate),
                    HKQuantityType.GetQuantityType(HKQuantityTypeIdentifierKey.BodyTemperature),
                    HKQuantityType.GetQuantityType(HKQuantityTypeIdentifierKey.RespiratoryRate),

                    HKQuantityType.GetQuantityType(HKQuantityTypeIdentifierKey.DistanceWalkingRunning),
                    HKQuantityType.GetQuantityType(HKQuantityTypeIdentifierKey.FlightsClimbed),
                    HKQuantityType.GetQuantityType(HKQuantityTypeIdentifierKey.StepCount),
                    HKQuantityType.GetQuantityType(HKQuantityTypeIdentifierKey.ActiveEnergyBurned),
                    HKQuantityType.GetQuantityType(HKQuantityTypeIdentifierKey.BasalEnergyBurned),
                    HKQuantityType.GetQuantityType(HKQuantityTypeIdentifierKey.NikeFuel),


                    HKCategoryType.GetCategoryType(HKCategoryTypeIdentifierKey.SleepAnalysis),
					HKQuantityType.GetQuantityType (HKQuantityTypeIdentifierKey.DietaryEnergyConsumed),
					HKQuantityType.GetQuantityType (HKQuantityTypeIdentifierKey.ActiveEnergyBurned),			
					HKCharacteristicType.GetCharacteristicType (HKCharacteristicTypeIdentifierKey.DateOfBirth)
				});
            }
        }

        public ProfileViewController(IntPtr handle)
            : base(handle)
        {
            Host = "http://216.195.85.166";
        }

        public override async void ViewWillAppear(bool animated)
        {
            base.ViewWillAppear(animated);
            if (HKHealthStore.IsHealthDataAvailable)
            {

                var success = await HealthStore.RequestAuthorizationToShareAsync(DataTypesToWrite, DataTypesToRead);

                if (!success.Item1)
                {
                    Console.WriteLine("You didn't allow HealthKit to access these read/write data types. " +
                    "In your app, try to handle this error gracefully when a user decides not to provide access. " +
                    "If you're using a simulator, try it on a device.");
                    return;
                }

                numberFormatter = new NSNumberFormatter();

                //UpdateUsersAge ();
                //UpdateUsersHeight ();
                //UpdateUsersWeight ();
            }
        }

        public override void ViewDidLoad()
        {
            base.ViewDidLoad();

            Send.TouchUpInside += delegate
            {
                SendBodyMeasurement();
                SendBaseData();
                SendFitnessActivity();
                SendSleep();
            };
        }

        void SendBodyMeasurement()
        {
            List<HKQuantityType> bodyMeasurementsType = new List<HKQuantityType>();
            bodyMeasurementsType.Add(HKQuantityType.GetQuantityType(HKQuantityTypeIdentifierKey.LeanBodyMass));
            bodyMeasurementsType.Add(HKQuantityType.GetQuantityType(HKQuantityTypeIdentifierKey.BodyMass));
            bodyMeasurementsType.Add(HKQuantityType.GetQuantityType(HKQuantityTypeIdentifierKey.BodyMassIndex));
            bodyMeasurementsType.Add(HKQuantityType.GetQuantityType(HKQuantityTypeIdentifierKey.BodyFatPercentage));
            bodyMeasurementsType.Add(HKQuantityType.GetQuantityType(HKQuantityTypeIdentifierKey.Height));
            foreach (HKQuantityType bodyMeasurement in bodyMeasurementsType)
            {
                FetchData(bodyMeasurement, (quantities, error) =>
                {
                    if (error != null)
                    {
                        Console.WriteLine("An error occured fetching the user's height information. " +
                        "In your app, try to handle this gracefully. The error was: {0}.", error.LocalizedDescription);
                        return;
                    }

                    if (quantities.Length != 0)
                    {
                        Int16 type = 0;
                        switch (bodyMeasurement.Description)
                        {
                            case "HKQuantityTypeIdentifierLeanBodyMass":
                                type = 0;
                                break;
                            case "HKQuantityTypeIdentifierBodyMass":
                                type = 1;
                                break;
                            case "HKQuantityTypeIdentifierBodyMassIndex":
                                type = 2;
                                break;
                            case "HKQuantityTypeIdentifierBodyFatPercentage":
                                type = 3;
                                break;
                            case "HKQuantityTypeIdentifierHeight":
                                type = 4;
                                break;
                        }
                        for (int i = 0; i < quantities.Length; i++)
                        {
                            var quantitySample = (HKQuantitySample)quantities[i];
                            String[] quntityVal = quantitySample.Quantity.Description.Split();
                            String unitVal = quntityVal[quntityVal.Length - 1];
                            HKUnit unit = HKUnit.FromString(unitVal);
                            double quantity = quantitySample.Quantity.GetDoubleValue(unit);
                            String source = quantitySample.Source.Name;

                            String timestamp = Convert.ToDateTime(quantitySample.StartDate.ToString()).ToShortDateString();

                            var host = Host;

                            var request =
                                (HttpWebRequest)
                                    WebRequest.Create(
                                        String.Format(
                                            @"{0}/fitTracker/HealhKitBodyMeasurement?userId={1}&measurement={2}&type={3}&timestamp={4}&source={5}",
                                            host, 999999, quantity, type, timestamp,
                                            source));

                            var data = Encoding.ASCII.GetBytes("");

                            request.Method = "GET";
                            request.ContentType = "application/x-www-form-urlencoded";
                            request.ContentLength = data.Length;
                            var response = (HttpWebResponse)request.GetResponse();
                        }

                    }
                });
            }
        }

        void SendBaseData()
        {
            List<HKQuantityType> baseDataType = new List<HKQuantityType>();
            baseDataType.Add(HKQuantityType.GetQuantityType(HKQuantityTypeIdentifierKey.BloodPressureDiastolic));
            baseDataType.Add(HKQuantityType.GetQuantityType(HKQuantityTypeIdentifierKey.BloodPressureSystolic));
            baseDataType.Add(HKQuantityType.GetQuantityType(HKQuantityTypeIdentifierKey.HeartRate));
            baseDataType.Add(HKQuantityType.GetQuantityType(HKQuantityTypeIdentifierKey.BodyTemperature));
            baseDataType.Add(HKQuantityType.GetQuantityType(HKQuantityTypeIdentifierKey.RespiratoryRate));

            foreach (HKQuantityType baseData in baseDataType)
            {
                FetchData(baseData, (quantities, error) =>
                {
                    if (error != null)
                    {
                        Console.WriteLine("An error occured fetching the user's height information. " +
                        "In your app, try to handle this gracefully. The error was: {0}.", error.LocalizedDescription);
                        return;
                    }

                    if (quantities.Length != 0)
                    {
                        Int16 type = 0;
                        switch (baseData.Description)
                        {
                            case "HKQuantityTypeIdentifierPressureDiastolic":
                            case "HKQuantityTypeIdentifierBloodPressureSystolic":
                                type = 0;
                                break;
                            case "HKQuantityTypeIdentifierHeartRate":
                                type = 1;
                                break;
                            case "HKQuantityTypeIdentifierBodyTemperature":
                                type = 2;
                                break;
                            case "HKQuantityTypeIdentifierRespiratoryRate":
                                type = 3;
                                break;
                        }
                        for (int i = 0; i < quantities.Length; i++)
                        {
                            var quantitySample = (HKQuantitySample)quantities[i];
                            String[] quntityVal = quantitySample.Quantity.Description.Split();
                            String unitVal = quntityVal[quntityVal.Length - 1];
                            HKUnit unit = HKUnit.FromString(unitVal);
                            double quantity = quantitySample.Quantity.GetDoubleValue(unit);
                            String source = quantitySample.Source.Name;

                            String timestamp = Convert.ToDateTime(quantitySample.StartDate.ToString()).ToShortDateString();

                            var host = Host;

                            var request =
                                (HttpWebRequest)
                                    WebRequest.Create(
                                        String.Format(
                                            @"{0}/fitTracker/HealhKitBaseData?userId={1}&measurement={2}&type={3}&timestamp={4}&source={5}",
                                            host, 999999, quantity, type, timestamp,
                                            source));

                            var data = Encoding.ASCII.GetBytes("");

                            request.Method = "GET";
                            request.ContentType = "application/x-www-form-urlencoded";
                            request.ContentLength = data.Length;
                            var response = (HttpWebResponse)request.GetResponse();
                        }

                    }
                });
            }
        }

        void SendFitnessActivity()
        {
            List<HKQuantityType> fitnessActivityType = new List<HKQuantityType>();
            fitnessActivityType.Add(HKQuantityType.GetQuantityType(HKQuantityTypeIdentifierKey.DistanceCycling));
            fitnessActivityType.Add(HKQuantityType.GetQuantityType(HKQuantityTypeIdentifierKey.DistanceWalkingRunning));
            fitnessActivityType.Add(HKQuantityType.GetQuantityType(HKQuantityTypeIdentifierKey.FlightsClimbed));
            fitnessActivityType.Add(HKQuantityType.GetQuantityType(HKQuantityTypeIdentifierKey.StepCount));
            fitnessActivityType.Add(HKQuantityType.GetQuantityType(HKQuantityTypeIdentifierKey.ActiveEnergyBurned));
            fitnessActivityType.Add(HKQuantityType.GetQuantityType(HKQuantityTypeIdentifierKey.BasalEnergyBurned));
            fitnessActivityType.Add(HKQuantityType.GetQuantityType(HKQuantityTypeIdentifierKey.NikeFuel));

            foreach (HKQuantityType fitnessActivity in fitnessActivityType)
            {
                FetchData(fitnessActivity, (quantities, error) =>
                {
                    if (error != null)
                    {
                        Console.WriteLine("An error occured fetching the user's height information. " +
                        "In your app, try to handle this gracefully. The error was: {0}.", error.LocalizedDescription);
                        return;
                    }

                    if (quantities.Length != 0)
                    {
                        Int16 type = 0;
                        switch (fitnessActivity.Description)
                        {
                            case "HKQuantityTypeIdentifierDistanceCycling":
                                type = 0;
                                break;
                            case "HKQuantityTypeIdentifierDistanceWalkingRunning":
                                type = 1;
                                break;
                            case "HKQuantityTypeIdentifierFlightsClimbed":
                                type = 2;
                                break;
                            case "HKQuantityTypeIdentifierStepCount":
                                type = 5;
                                break;
                            case "HKQuantityTypeIdentifierActiveEnergyBurned":
                                type = 6;
                                break;
                            case "HKQuantityTypeIdentifierBasalEnergyBurned":
                                type = 7;
                                break;
                            case "HKQuantityTypeIdentifierNikeFuel":
                                type = 8;
                                break;
                        }
                        for (int i = 0; i < quantities.Length; i++)
                        {
                            var quantitySample = (HKQuantitySample)quantities[i];
                            String[] quntityVal = quantitySample.Quantity.Description.Split();
                            String unitVal = quntityVal[quntityVal.Length - 1];
                            HKUnit unit = HKUnit.FromString(unitVal);
                            double quantity = quantitySample.Quantity.GetDoubleValue(unit);
                            String source = quantitySample.Source.Name;

                            String timestamp = Convert.ToDateTime(quantitySample.StartDate.ToString()).ToShortDateString();

                            var host = Host;

                            var request =
                                (HttpWebRequest)
                                    WebRequest.Create(
                                        String.Format(
                                            @"{0}/fitTracker/HealhKitFitnessActivity?userId={1}&measurement={2}&type={3}&timestamp={4}&source={5}",
                                            host, 999999, quantity, type, timestamp,
                                            source));

                            var data = Encoding.ASCII.GetBytes("");

                            request.Method = "GET";
                            request.ContentType = "application/x-www-form-urlencoded";
                            request.ContentLength = data.Length;
                            var response = (HttpWebResponse)request.GetResponse();
                        }

                    }
                });
            }
        }

        void SendSleep()
        {
            List<HKCategoryType> sleepType = new List<HKCategoryType>();
            sleepType.Add(HKCategoryType.GetCategoryType(HKCategoryTypeIdentifierKey.SleepAnalysis));

            foreach (HKCategoryType sleep in sleepType)
            {
                FetchData2(sleep, (quantities, error) =>
                {
                    if (error != null)
                    {
                        Console.WriteLine("An error occured fetching the user's height information. " +
                        "In your app, try to handle this gracefully. The error was: {0}.", error.LocalizedDescription);
                        return;
                    }

                    if (quantities.Length != 0)
                    {
                        Int16 type = 0;
                        switch (sleep.Description)
                        {
                            case "HKQuantityTypeIdentifierDistanceCycling":
                                type = 0;
                                break;
                            case "HKQuantityTypeIdentifierDistanceWalkingRunning":
                                type = 1;
                                break;
                        }
                        for (int i = 0; i < quantities.Length; i++)
                        {
                            var quantitySample = (HKQuantitySample)quantities[i];
                            String[] quntityVal = quantitySample.Quantity.Description.Split();
                            String unitVal = quntityVal[quntityVal.Length - 1];
                            HKUnit unit = HKUnit.FromString(unitVal);
                            double quantity = quantitySample.Quantity.GetDoubleValue(unit);
                            String source = quantitySample.Source.Name;

                            String timestamp = Convert.ToDateTime(quantitySample.StartDate.ToString()).ToShortDateString();

                            var host = Host;

                            var request =
                                (HttpWebRequest)
                                    WebRequest.Create(
                                        String.Format(
                                            @"{0}/fitTracker/HealhKitSleep?userId={1}&measurement={2}&type={3}&timestamp={4}&source={5}",
                                            host, 999999, quantity, type, timestamp,
                                            source));

                            var data = Encoding.ASCII.GetBytes("");

                            request.Method = "GET";
                            request.ContentType = "application/x-www-form-urlencoded";
                            request.ContentLength = data.Length;
                            var response = (HttpWebResponse)request.GetResponse();
                        }

                    }
                });
            }
        }

        //void UpdateUsersAge ()
        //{
        //    NSError error;
        //    NSDate dateOfBirth = HealthStore.GetDateOfBirth (out error);

        //    if (error != null) {
        //        Console.WriteLine ("An error occured fetching the user's age information. " +
        //        "In your app, try to handle this gracefully. The error was: {0}", error);
        //        return;
        //    }

        //    if (dateOfBirth == null)
        //        return;

        //    var now = NSDate.Now;

        //    NSDateComponents ageComponents = NSCalendar.CurrentCalendar.Components (NSCalendarUnit.Year, dateOfBirth, now,
        //                                         NSCalendarOptions.WrapCalendarComponents);

        //    nint usersAge = ageComponents.Year;

        //    ageHeightValueLabel.Text = string.Format ("{0} years", usersAge);
        //}

        //void UpdateUsersHeight ()
        //{
        //    var heightType = HKQuantityType.GetQuantityType (HKQuantityTypeIdentifierKey.Height);

        //    FetchMostRecentData (heightType, (mostRecentQuantity, error) => {
        //        if (error != null) {
        //            Console.WriteLine ("An error occured fetching the user's height information. " +
        //            "In your app, try to handle this gracefully. The error was: {0}.", error.LocalizedDescription);
        //            return;
        //        }

        //        double usersHeight = 0.0;

        //        if (mostRecentQuantity != null) {
        //            var heightUnit = HKUnit.Inch;
        //            usersHeight = mostRecentQuantity.GetDoubleValue (heightUnit);
        //        }

        //        InvokeOnMainThread (delegate {
        //            heightValueLabel.Text = numberFormatter.StringFromNumber (new NSNumber (usersHeight));
        //        });
        //    });
        //}

        //void UpdateUsersWeight ()
        //{
        //    var weightType = HKQuantityType.GetQuantityType (HKQuantityTypeIdentifierKey.BodyMass);

        //    FetchMostRecentData (weightType, (mostRecentQuantity, error) => {
        //        if (error != null) {
        //            Console.WriteLine ("An error occured fetching the user's age information. " +
        //            "In your app, try to handle this gracefully. The error was: {0}", error.LocalizedDescription);
        //            return;
        //        }

        //        double usersWeight = 0.0;

        //        if (mostRecentQuantity != null) {
        //            var weightUnit = HKUnit.Pound;
        //            usersWeight = mostRecentQuantity.GetDoubleValue (weightUnit);
        //        }

        //        InvokeOnMainThread (delegate {
        //            weightValueLabel.Text = numberFormatter.StringFromNumber (new NSNumber (usersWeight));
        //        });
        //    }
        //    );
        //}

        //void FetchMostRecentData (HKQuantityType quantityType, Action <HKQuantity, NSError> completion)
        //{
        //    var timeSortDescriptor = new NSSortDescriptor (HKSample.SortIdentifierEndDate, false);
        //    var query = new HKSampleQuery (quantityType, null, 1, new NSSortDescriptor[] { timeSortDescriptor },
        //                    (HKSampleQuery resultQuery, HKSample[] results, NSError error) => {
        //            if (completion != null && error != null) {
        //                completion (null, error);
        //                return;
        //            }

        //            HKQuantity quantity = null;
        //            if (results.Length != 0) {
        //                var quantitySample = (HKQuantitySample)results [results.Length - 1];
        //                quantity = quantitySample.Quantity;

        //            }

        //            if (completion != null)
        //                completion (quantity, error);
        //        });

        //    HealthStore.ExecuteQuery (query);
        //}

        void FetchData(HKQuantityType quantityType, Action<HKSample[], NSError> completion)
        {
            var timeSortDescriptor = new NSSortDescriptor(HKSample.SortIdentifierEndDate, false);
            var query = new HKSampleQuery(quantityType, null, 100, null,
                            (HKSampleQuery resultQuery, HKSample[] results, NSError error) =>
                            {
                                if (completion != null && error != null)
                                {
                                    completion(null, error);
                                    return;
                                }

                                if (completion != null)
                                    completion(results, error);
                            });

            HealthStore.ExecuteQuery(query);
        }

        void FetchData2(HKCategoryType quantityType, Action<HKSample[], NSError> completion)
        {
            var timeSortDescriptor = new NSSortDescriptor(HKSample.SortIdentifierEndDate, false);
            var query = new HKSampleQuery(quantityType, null, 100, null,
                            (HKSampleQuery resultQuery, HKSample[] results, NSError error) =>
                            {
                                if (completion != null && error != null)
                                {
                                    completion(null, error);
                                    return;
                                }

                                if (completion != null)
                                    completion(results, error);
                            });

            HealthStore.ExecuteQuery(query);
        }

        //void SaveHeightIntoHealthStore (double value)
        //{
        //    var heightQuantity = HKQuantity.FromQuantity (HKUnit.Inch, value);
        //    var heightType = HKQuantityType.GetQuantityType (HKQuantityTypeIdentifierKey.Height);
        //    var heightSample = HKQuantitySample.FromType (heightType, heightQuantity, NSDate.Now, NSDate.Now, new NSDictionary ());

        //    HealthStore.SaveObject (heightSample, (success, error) => {
        //        if (!success) {
        //            Console.WriteLine ("An error occured saving the height sample {0}. " +
        //            "In your app, try to handle this gracefully. The error was: {1}.", heightSample, error);
        //            return;
        //        }

        //        UpdateUsersHeight ();
        //    });
        //}

        //void SaveWeightIntoHealthStore (double value)
        //{
        //    var weightQuantity = HKQuantity.FromQuantity (HKUnit.Pound, value);
        //    var weightType = HKQuantityType.GetQuantityType (HKQuantityTypeIdentifierKey.BodyMass);
        //    var weightSample = HKQuantitySample.FromType (weightType, weightQuantity, NSDate.Now, NSDate.Now, new NSDictionary ());

        //    HealthStore.SaveObject (weightSample, (success, error) => {
        //        if (!success) {
        //            Console.WriteLine ("An error occured saving the weight sample {0}. " +
        //                "In your app, try to handle this gracefully. The error was: {1}.", weightSample, error.LocalizedDescription);
        //            return;
        //        }

        //        UpdateUsersWeight ();
        //    });
        //}

        //public override void RowSelected (UITableView tableView, NSIndexPath indexPath)
        //{
        //    string title = string.Empty;
        //    Action<double> valueChangedHandler = null;
        //    if (indexPath.Row == 0)
        //        return;

        //    switch (indexPath.Row) {
        //    case 1:
        //        title = "Your Height";
        //        valueChangedHandler = SaveHeightIntoHealthStore;
        //        break;
        //    case 2:
        //        title = "Your Weight";
        //        valueChangedHandler = SaveWeightIntoHealthStore;
        //        break;
        //    }

        //    var alertController = UIAlertController.Create (title, string.Empty, UIAlertControllerStyle.Alert);
        //    alertController.AddTextField ((textField) => {
        //        textField.KeyboardType = UIKeyboardType.DecimalPad;
        //    });

        //    var okAction = UIAlertAction.Create ("OK", UIAlertActionStyle.Default, (alertAction) => {
        //        var textField = alertController.TextFields [0];
        //        double value;
        //        Double.TryParse (textField.Text, out value);
        //        if(valueChangedHandler != null)
        //            valueChangedHandler (value);
        //        TableView.DeselectRow (indexPath, true);
        //    });

        //    alertController.AddAction (okAction);

        //    var cancelAction = UIAlertAction.Create ("Cancel", UIAlertActionStyle.Cancel, (alertAction) => {
        //        TableView.DeselectRow (indexPath, true);
        //    });

        //    alertController.AddAction (cancelAction);
        //    PresentViewController (alertController, true, null);
        //}
    }
}
