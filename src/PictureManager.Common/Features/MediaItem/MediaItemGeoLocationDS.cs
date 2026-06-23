using MH.Utils.DB;
using MH.Utils.DB.DataSources;
using MH.Utils.DB.Relations;
using PictureManager.Common.Features.GeoLocation;
using System.Collections.Generic;
using System.Linq;

namespace PictureManager.Common.Features.MediaItem;

public sealed class MediaItemGeoLocationDS : CsvOneToOneDataSource<MediaItemM, GeoLocationM> {
  public MediaItemGeoLocationDS(SimpleDB db, MediaItemDS dsA, GeoLocationDS dsB, IRelation relation)
    : base(db, "MediaItemGeoLocation", dsA, dsB, relation) {

    IsDriveRelated = true;
  }

  protected override Dictionary<string, IEnumerable<KeyValuePair<MediaItemM, GeoLocationM>>> _getAsDriveRelated() =>
    CoreR.GetAsDriveRelated(
      DataSourceA.Repository
        .GetAll(x => x.GeoLocation != null)
        .Select(x => new KeyValuePair<MediaItemM, GeoLocationM>(x, x.GeoLocation!)),
      x => x.Key.Folder);

  protected override void _link(KeyValuePair<MediaItemM, GeoLocationM> item) =>
    item.Key.GeoLocation = item.Value;
}