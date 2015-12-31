'use strict';

var ChinehookApp = (function() {

   var /** @type {ChinehookApp.core.User} */ currentUser;

   /**
    * Check the current user connected
    * @returns {Promise.<boolean>}
    */
   function checkConnection() {
      return AdminLib.loadAjax("session").then(function(data) {

         if (data.user === undefined)
            return false;

         if (data.user.id === -1)
            return false;

         updateCurrentUser(data.user);
         return true;
      });
   }

   /**
    * Connect the user to it's session :
    *    - If the user is already connect, then nothing is done
    *    - If no user is connected, then display the login screen
    * @returns {Promise<U>}
    */
   function connectUser() {

      return checkConnection()
               .then(
                  function(connected) {
                     if (connected)
                        return;

                     if (AdminLib.StandardPage !== undefined)
                        AdminLib.StandardPage.dispose();

                     return loginModule.displayScreen();

                  })
               .then(checkConnection);

   }

   function displayDashboard() {
      return ChinehookApp.page.home.display();
   }

   /**
    *
    * @returns {AdminLib.User}
    */
   function getCurrentUser() {
      return currentUser;
   }

   /**
    * Login the given user.
    * The function will call the "/user/connect" api.
    * If the login goes well, the server will response with a status 200 (OK).
    * If the login was denied, the server will response with a status 403 (Forbiden).
    * Any other code will raised an exception with the server response as error.
    *
    * @param {string} username
    * @param {string} password
    * @returns {Promise.<boolean>} The promise will return true if the username/password is valid. If invalid, will return false. If an error occurs with the ajax request, it will raise the server response.
    */
   function login(username, password) {

      var /** @type {function} */ fulfill
        , /** @type {function} */ reject;

      // Function that will be executed if the login goes well (login/password valid)
      fulfill = function() { return true; };

      // Function executed if the login goes wrong : the server will reject the request
      // by a 403 error.
      reject  = function(response) {
         if (response.status === 403)
            return false;

         throw response;
      };

      return AdminLib.loadAjax('user/connect', {username: username, password: password}, 'POST')
               .then(fulfill, reject);

   }

   function logout() {
      currentUser = undefined;
      return AdminLib.loadAjax('user/disconnect')
         .then(connectUser)
         .then(
            function() {
               AdminLib.StandardPage.display();
               displayDashboard();
            })
   }

   function start() {

      // The application start once the AdminLib have been initialized
      AdminLib
         .then(loadConfig)
         .then(initialize)
         .then(function() {
             return connectUser();
         })
         .then(function() {
            AdminLib.StandardPage.display();
            displayDashboard();
         });

   }

   function updateCurrentUser(user) {

      currentUser = new AdminLib.User(user.id, user.username, user.email);

      currentUser.refresh().then(function() {

         var /** @type {Event} */ event;

         event = new AdminLib.Event(AdminLib.EVENT_TYPES.user_refresh, { cancelable : false
                                                                       , target     : currentUser });

         AdminLib.dispatchEvent(event);
      });
   }

   function loadConfig() {

      return AdminLib.loadScripts('ChinehookApp.config.js');

   }

   /**
    * Initializing the models.
    * The function will check that each model has been declared in the database.
    * @returns {Promise.<boolean>}
    */
   function initialize() {
      moment.locale('en-GB');
   }

   /**
    * @name ModelList
    * @typedef {Object.<ModelList>|ModelList[]|AdminLib.Model}
    *
    */

   var loginModule = (function() {

      /**
       * Display the login screen.
       * The function will return a promise that will be fulfilled only
       * when the login is fully completed : the promise will not be fulfilled
       * until the user enter a valid username/password and has been connected by
       * the server.
       * @returns {Promise}
       */
      function display() {

         var /** @type {HTMLElement} */ body
           , /** @type {string}      */ domString
              , /** @type {Promise}  */ promise
           , whenLoginFunction;

         domString = Mustache.render(AdminLib.getTemplate('login'), {metronicRoot : AdminLib.METRONIC_ROOT});

         body = document.querySelector('body');

         AdminLib.dom.empty(body);

         body.innerHTML = domString;

         body.classList.add('login');

         $('form.login-form').submit(function(event) {

            var /** @type {function} */ fulfill
              , /** @type {function} */ reject
              , /** @type {boolean}  */ isError
              , /** @type {string}   */ password
              , /** @type {string}   */ username;

            event.preventDefault();
            event.stopImmediatePropagation();

            username = event.target.querySelector('input[name="username"]').value;
            password = event.target.querySelector('input[name="password"]').value;

            // Both fields (username & password) are required
            isError =    username === undefined || password === undefined || username === "" || password === "";

            // If one of the field is invalid, we show an error an return
            if (isError) {
               $('.alert-error', $('.login-form')).show();
               return false;
            }

            $('.alert-error', $('.login-form')).hide();

            // Function executed when the login is finished.
            // The login function return a boolean that indicate if
            // the login is successful (true) or not (false).
            // If the login is successful, we fullfil the Promise that we had previously
            // created.
            fulfill = function(connected) {

               if (!connected) {
                  $('#loginFail', $('.login-form')).show();
                  return;
               }

               AdminLib.dom.empty(document.body);
               document.body.classList.remove('login');

               whenLoginFunction(); // Resolving the promise

            }.bind(this);

            // Function executed if the login function as raised an exception
            reject = function() {
               $('#internalError', $('.login-form')).show();
            };

            login(username, password)
               .then(fulfill, reject);

            return false;
         }.bind(this));

         initialize();

         promise = new Promise(function(fullfil) {
            whenLoginFunction = fullfil;
         });

         return promise;
      };

      function displayScreen() {

         return load()
                  .then(
                     function() {
                        return display();
                     });

      };

      /**
       * Initialize the screen
       */
      function initialize() {

         AdminLib.metronic.login.init();

          // init background slide images
         $.backstretch ( [ AdminLib.METRONIC_ROOT + '/assets/admin/pages/media/bg/1.jpg'
                         , AdminLib.METRONIC_ROOT + '/assets/admin/pages/media/bg/2.jpg'
                         , AdminLib.METRONIC_ROOT + '/assets/admin/pages/media/bg/3.jpg'
                         , AdminLib.METRONIC_ROOT + '/assets/admin/pages/media/bg/4.jpg']
                       , { fade     : 1000
                         , duration : 8000});

      }

      function load() {
         return AdminLib.loadModule('ChinehookApp.login')
      }

      return {displayScreen : displayScreen };
   })();

   var ChinehookApp = { APP_ROOT        : AdminLib.SERVER_ROOT
                      , EVENT_TYPES     : {user_refresh : 'user refresh'}
                      , PAGE_ROOT       : '/static/script/page'
                      , module          : { app  : { job : {} }
                                          , load : {centerGroup : { database : {} } } }
                      , page            : {}
                      , session         : { disconnect      : logout
                                          , getCurrentUser  : getCurrentUser
                                          , connect         : connectUser
                                          , check           : checkConnection } };

   ChinehookApp.SCRIPT_ROOT = AdminLib.SCRIPT_ROOT;

   start();

   return ChinehookApp;
})();