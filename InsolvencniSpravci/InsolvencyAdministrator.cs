using System;
using System.Collections.Generic;

namespace InsolvencniSpravci
{
	class InsolvencyAdministrator
	{
		public int Id { get; set; }
		public string LinkDetail { get; set; }
		public string Name { get; set; }
		public string ICO { get; set; }
		public string Type { get; set; }

		public string OldNames { get; set; }

		public string FirstName { get; set; }
		public string SureName { get; set; }
		public string OldSureNames { get; set; }
		public DateTime? BirthDate { get; set; }
		public bool CanDebtRelief { get; set; }
		public bool CanKonkurs { get; set; }

		public string Street { get; set; }
		public string Zip { get; set; }
		public string Number { get; set; }
		public string City { get; set; }
		public string District { get; set; }

		public DateTime? DateOfEstablishment { get; set; }
		public DateTime? DateOfExam { get; set; }
		public DateTime? DateOfSuspension { get; set; }
		public string ReasonOfSuspension { get; set; }
		public DateTime? DateOfTermination { get; set; }
		public string ReasonOfTermination { get; set; }

		public List<Companion> Companions { get; set; }
		public List<BranchOffice> BranchOffices { get; set; }

		public InsolvencyAdministrator()
		{
			Companions = new List<Companion>();
			BranchOffices = new List<BranchOffice>();
		}
	}
}
