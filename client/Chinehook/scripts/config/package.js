'use strict';

/** Configuration file of application */

(function() {

   var /** @type {AdminLib.Repository} */ repository;


   repository = new AdminLib.Repository( 'ChinehookApp'
                                      , { templateFolder   : ChinehookApp.APP_ROOT + '/template'
                                        , stylesheetFolder : ChinehookApp.APP_ROOT + '/stylesheet'
                                        , scriptFolder     : ChinehookApp.SCRIPT_ROOT });

   // ChinehookApp
   repository.addPackage('ChinehookApp'      , { packages  : ['AdminLib', 'ChinehookApp.core']
                                               , templates : ['standardView']});

   // ChinehookApp.core
   repository.addPackage('ChinehookApp.core' , { packages : ['ChinehookApp.portlet'] });

   // ChinehookApp.login
   repository.addPackage('ChinehookApp.login', { packages  : ['jQuery validation', 'backstretch', 'select2', 'metronic.login']
                                               , templates : [ 'login']});

   repository.save();

})();