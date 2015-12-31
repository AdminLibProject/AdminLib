'use strict';

/**
 *
 * @typedef {Object} Chinehook.models.Artist
 * @property {Chinehook.models.Album[]} albums
 * @property {string}                   code
 * @property {number}                   id
 * @property {string}                   label
 *
 */

(function() {

   var /** @type {AdminLib.Model.Handler}            */ modelHandler
     , /** @type {AdminLib.Model.Handler.Parameters} */ parameters;

   parameters =
      { create           : true

      , delete           : { confirmation : true }

      , itemType         : 'artist'
      , itemType_plural  : 'artists'

      , fields           : [
                             // Label
                             { code        : 'name'
                             , linkToItem  : true
                             , required    : true }]

      , includeApiFields : ['id'] };

   modelHandler = AdminLib.model('artist').defineHandler(parameters);

})();