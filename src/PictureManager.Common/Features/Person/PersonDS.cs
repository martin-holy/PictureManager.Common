using MH.Utils.DB;
using MH.Utils.DB.DataSources;
using MH.Utils.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;

namespace PictureManager.Common.Features.Person;

/// <summary>
/// DB fields: ID|Name|Segments|Keywords
/// </summary>
public sealed class PersonDS : CsvTreeDataSource<PersonM, PersonR, PersonLinkInfo> {
  private readonly CoreR _coreR;
  private readonly Dictionary<PersonM, List<int>> _notAvailableTopSegments = [];
  private const string _notFoundRecordNamePrefix = "Not found ";

  public PersonDS(CoreR coreR, PersonR repo) : base(coreR.DB, "People", 4, repo) {
    _coreR = coreR;
  }

  public override bool Save() =>
    _saveToSingleFile(Repository.GetAll());

  protected override (PersonM item, PersonLinkInfo linkInfo) _fromCsv(ReadOnlySpan<char> csv) {
    int start = 0;
    int field = 0;

    int id = 0;
    string name = string.Empty;
    string segments = string.Empty;
    string keywords = string.Empty;

    for (int i = 0; i <= csv.Length; i++) {
      if (i == csv.Length || csv[i] == '|') {
        var slice = csv[start..i];

        switch (field) {
          case 0: id = CsvParser.ParseInt(slice); break;
          case 1: name = slice.ToString(); break;
          case 2: segments = slice.ToString(); break;
          case 3: keywords = slice.ToString(); break;
        }

        field++;
        start = i + 1;
      }
    }

    _validateFieldsCount(field, csv);

    var item = new PersonM(id, name);
    if (item.Name.StartsWith(PersonR.UnknownPersonNamePrefix))
      item.IsUnknown = true;

    var linkInfo = new PersonLinkInfo(segments, keywords);
    return new(item, linkInfo);
  }

  protected override string _toCsv(PersonM person) =>
    string.Join("|",
      person.GetHashCode().ToString(),
      person.Name,
      _topSegmentsToCsv(person),
      person.Keywords.ToHashCodes().ToCsv());

  public override void LinkReferences() {
    _coreR.CategoryGroup.DataSource.LinkGroups(Repository.Tree, AllDict);

    foreach (var (person, li) in _allLinkInfo) {
      // top segments
      CsvParser.ParseInts(li.Segments, (person, _coreR.Segment.DataSource.AllDict, _notAvailableTopSegments), static (state, id) => {
        if (state.AllDict.TryGetValue(id, out var rec)) {
          state.person.TopSegments ??= [];
          state.person.TopSegments.Add(rec);
          state.person.Segment = state.person.TopSegments[0];
        }
        else {
          if (state._notAvailableTopSegments.TryGetValue(state.person, out var segments))
            segments.Add(id);
          else
            state._notAvailableTopSegments.Add(state.person, [id]);
        }
      });

      // reference to Keywords
      person.Keywords = _coreR.Keyword.DataSource.Link(li.Keywords, this);

      // add loose people
      foreach (var personM in AllDict.Values.Where(x => x.Parent == null)) {
        personM.Parent = Repository.Tree;
        personM.Parent.Items.Add(personM);
      }
    }
  }

  public List<PersonM>? Link(string csv, ICsvRepositoryDataSource seeker) =>
    LinkList(csv, _getNotFoundRecord, seeker);

  public PersonM GetPerson(int id, ICsvRepositoryDataSource seeker) =>
    AllDict.TryGetValue(id, out var person)
      ? person
      : _resolveNotFoundRecord(id, _getNotFoundRecord, seeker)!;

  // the sort order for not available will be lost so take available first
  private string _topSegmentsToCsv(PersonM person) =>
    _notAvailableTopSegments.TryGetValue(person, out var ts)
      ? (person.TopSegments.ToHashCodes() ?? Array.Empty<int>()).Concat(ts).ToCsv()
      : person.TopSegments.ToHashCodes().ToCsv();

  private PersonM _getNotFoundRecord(int notFoundId) {
    var id = Repository.GetNextId();
    var item = new PersonM(id, $"{_notFoundRecordNamePrefix}{id} ({notFoundId})") {
      Parent = Repository.Tree
    };
    item.Parent.Items.Add(item);
    Repository.IsModified = true;
    return item;
  }
}

public readonly struct PersonLinkInfo(string segments, string keywords) {
  public readonly string Segments = segments;
  public readonly string Keywords = keywords;
}