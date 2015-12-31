'use strict';

AdminLib.menu.declare(
   [
     // Music
     { label   : 'Music'
     , entries : [
                   // Artist
                   { page : 'album'
                   , label: 'Album'}

                   // Album
                 , { page : 'artist'
                   , label: 'Artist'}

                   // Genre
                 , { page : 'genre'
                   , label: 'Genre'}

                  // Playlist
                 , { page : 'playlist'
                   , label: 'Playlist'}

                  // Media type
                , { page  : 'mediaType'
                  , label : 'Media type'} ] }

     // store
   , { label   : 'Store'
     , entries : [
                    // Employee
                    { page  : 'employee'
                    , label : 'Employee'}

                    // Customer
                 ,  { page  : 'customer'
                    , label : 'Customer'}

                    // Invoice
                 , { page  : 'invoice'
                   , label : 'Invoice'} ] } ]);
