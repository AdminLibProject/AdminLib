'use strict';

/**
 *
 * @typedef {Object} Chinehook.models.Album
 * @property {number}                   id
 * @property {string}                   title
 * @property {Chinehook.models.Track[]} tracks
 * @property {Chinehook.models.Artist}  artist
 *
 */

(function() {

   var /** @type {AdminLib.Model.Handler}            */ modelHandler
     , /** @type {AdminLib.Model.Handler.Parameters} */ parameters
     , views;

   views                                         = (function() {

      var informations                           = (function() {

         var portlets                            = (function() {

            var informations                     = (function()   {

               /**
                * @param {Item} item
                * @returns {AdminLib.widget.Portlet.Editable}
                */
               function create(item) {

                  var /** @type {AdminLib.widget.Portlet.Editable} */ portlet;

                  portlet = new AdminLib.widget.Portlet.Editable({ data  : item
                                                                 , form  : modelHandler.getForm({item : item})
                                                                 , model : 'album'
                                                                 , title : 'Informations'});

                  return portlet;
               }

               return { create : create };
             })();

            var tracks                           = (function() {

               /**
                *
                * @param {Chinehook.models.Album} item
                * @returns {AdminLib.widget.Portlet}
                */
               function create(item) {
                  var /** @type {AdminLib.widget.Portlet}   */ portlet
                    , /** @type {AdminLib.widget.Datatable} */ datatable;

                  portlet = new AdminLib.widget.Portlet({ title : 'tracks'});

                  datatable = new AdminLib.widget.Datatable({ data   :  item.tracks
                                                            , fields : [
                                                                         // Name
                                                                         { code        : 'name'
                                                                         , label       : 'Title'
                                                                         , link        : { model : 'track'} }

                                                                         // media type
                                                                       , { code        : 'mediaType'
                                                                         , formatValue : 'name'
                                                                         , label       : 'Media type'
                                                                         , link        : { model : 'mediaType' }}

                                                                         // genre
                                                                       , { code        : 'genre'
                                                                         , formatValue : 'name'
                                                                         , link        : { model : 'genre' }}

                                                                         // composer
                                                                       , { code        : 'composer'}

                                                                         // Duration
                                                                       , { code        : 'duration'
                                                                         , formatValue : function(value) {
                                                                              return value / 1000;
                                                                           }
                                                                         , label       : 'Duration (s)'}

                                                                         // Price
                                                                       , { code        : 'price'
                                                                         , label       : 'Price ($)'} ]});

                  portlet.setContent(datatable);

                  return portlet;
               }

               return {create : create }

            })();

            return { tracks       : tracks
                   , informations : informations };

         })();

         /**
          *
          * @param {HTMLElement} dom
          * @param {Item}        item
          */
         function handler(dom, item) {
            var /** @type {AdminLib.widget.Portlet}   */ tracksPortlet
              , /** @type {AdminLib.widget.Portlet}   */ informationsPortlet;

            // Portlet: informations
            informationsPortlet = portlets.informations.create(item);

            // Portlet: albums
            tracksPortlet = portlets.tracks.create(item);

            dom.appendChild(informationsPortlet.getDOM());
            dom.appendChild(tracksPortlet.getDOM());

            return [informationsPortlet, tracksPortlet];
         }

         return { getData : getData
                , handler : handler };

      })();

      return {informations : informations };

   })();

   parameters =
      { create           : true
      , edit             : true

      , delete           : { confirmation : true }

      , itemType         : 'album'

      , fields           : [
                             // Label
                             { code        : 'name'
                             , linkToItem  : true
                             , required    : true }]

      , includeApiFields : ['id']

      , views            : [
                             // Informations
                             { code       : 'information'
                             , label      : 'Information'
                             , handler    : views.informations.handler}
                           ]};

   modelHandler = AdminLib.model('artist').defineHandler(parameters);

})();