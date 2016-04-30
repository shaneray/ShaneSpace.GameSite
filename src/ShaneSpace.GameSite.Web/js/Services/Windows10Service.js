angular.module("Windows10Service", [])
    .factory('Win10', [
    function () {
        
        var output = {
            ShowToast: function (title,text) {
                if(typeof Windows !== undefined) {
                    var notifications = Windows.UI.Notifications;
                    var template = notifications.ToastTemplateType.toastImageAndText04;
                    var toastXml = notifications.ToastNotificationManager.getTemplateContent(template);
                    var toastTextElements = toastXml.getElementsByTagName("text");
                    toastTextElements[0].appendChild(toastXml.createTextNode(title));
                    toastTextElements[1].appendChild(toastXml.createTextNode(text));
                    var toastImageElements = toastXml.getElementsByTagName("image");
                    toastImageElements[0].setAttribute("src", "images/touch-icon-iphone.png");
                    toastImageElements[0].setAttribute("alt", "red graphic");
                    var toast = new notifications.ToastNotification(toastXml);
                    var toastNotifier = notifications.ToastNotificationManager.createToastNotifier();
                    toastNotifier.show(toast);
                }
            }
        };

        return output;
    }]);