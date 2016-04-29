﻿using System;
using System.Collections.Generic;
using System.Linq;

using CollegeFbsRankings.Teams;

namespace CollegeFbsRankings.Conferences
{
    public class FbsConference
    {
        private readonly string _name;
        private readonly List<FbsTeam> _teams;
        private readonly List<FbsDivision> _divisions;

        private FbsConference(string name)
        {
            _name = name;
            _teams = new List<FbsTeam>();
            _divisions = new List<FbsDivision>();
        }

        public static FbsConference Create(string name)
        {
            var conference = new FbsConference(name);
            return conference;
        }

        public string Name
        {
            get { return _name; }
        }

        public IEnumerable<FbsTeam> Teams
        {
            get
            {
                if (_divisions.Any())
                    return _divisions.SelectMany(d => d.Teams);

                return _teams;
            }
        }

        public void AddTeam(FbsTeam team)
        {
            if (!_divisions.Any())
            {
                if (team.Conference == this)
                    _teams.Add(team);
                else
                {
                    throw new Exception(String.Format(
                        "Cannot add team {0} to conference {1} since team is already assigned to conference {2}.",
                        team.Name, Name, team.Conference.Name));
                }
            }
            else
            {
                throw new Exception(String.Format(
                    "Cannot add team {0} to conference {1} without assigning them to a division",
                    team.Name, Name));
            }
        }

        public void RemoveTeam(FbsTeam team)
        {
            if (!_divisions.Any())
                _teams.RemoveAll(t => t.Name == team.Name);
            else
            {
                foreach (var division in _divisions)
                    division.RemoveTeam(team);
            }
        }

        public IEnumerable<FbsDivision> Divisions
        {
            get { return _divisions; }
        }

        public void AddDivision(FbsDivision division)
        {
            if (!_teams.Any())
            {
                if (division.Conference == this)
                    _divisions.Add(division);
                else
                {
                    throw new Exception(String.Format(
                        "Cannot add division {0} to conference {1} since division is already assigned to conference {2}.",
                        division.Name, Name, division.Conference.Name));
                }
            }
            else
            {
                throw new Exception(String.Format(
                    "Cannot add division {0} to conference {1} since the conference already contains teams that are not assigned to a division",
                    division.Name, Name));
            }
        }
    }
}