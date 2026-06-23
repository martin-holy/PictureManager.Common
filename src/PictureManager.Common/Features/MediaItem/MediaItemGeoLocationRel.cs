using MH.Utils.DB.Relations;
using PictureManager.Common.Features.GeoLocation;
using System.Collections.Generic;

namespace PictureManager.Common.Features.MediaItem;

public sealed class MediaItemGeoLocationRel : Relation<MediaItemM, GeoLocationM> {
  public MediaItemGeoLocationDS DataSource { get; }

  public MediaItemGeoLocationRel(CoreR coreR, MediaItemR mediaItemR, GeoLocationR geoLocationR) : base(mediaItemR, geoLocationR) {
    DataSource = new(coreR.DB, mediaItemR.DataSource, geoLocationR.DataSource, this);
  }

  public void ItemUpdate(KeyValuePair<MediaItemM, GeoLocationM?> item) {
    if (ReferenceEquals(item.Key.GeoLocation, item.Value)) return;
    item.Key.GeoLocation = item.Value;
    IsModified = true;
    RepositoryA.Modify(item.Key);
  }
}